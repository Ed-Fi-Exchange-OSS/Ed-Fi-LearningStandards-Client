// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Auth;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EdFi.Admin.LearningStandards.Core.Services
{
    public class LearningStandardsSynchronizer : ILearningStandardsSynchronizer, ILearningStandardsChangesAvailable
    {
        private const int DescriptorBaseProgressPercentage = 0;

        private const int DescriptorLimitProgressPercentage = DescriptorMaxProgressPercentage - 1;

        private const int DescriptorMaxProgressPercentage = 1;

        // This expresses the implicit link between the sync sub-processes.
        private const int LearningStandardsBaseProgressPercentage = DescriptorMaxProgressPercentage;

        private const int LearningStandardsLimitProgressPercentage = LearningStandardsMaxProgressPercentage - 1;

        private const int LearningStandardsMaxProgressPercentage = 100;

        private const int LearningStandardRemainingSyncProcessPercentage = LearningStandardsMaxProgressPercentage - LearningStandardsBaseProgressPercentage;

        private readonly IEdFiBulkJsonPersisterFactory _bulkJsonPersisterFactory;

        private readonly ConcurrentDictionary<Guid, int> _countsByProcessId = new ConcurrentDictionary<Guid, int>();

        private readonly ILearningStandardsDataRetriever _learningStandardsDataRetriever;

        private readonly ILearningStandardsProviderAuthTokenManagerFactory _learningStandardsProviderAuthTokenManagerFactory;
        private readonly IChangeSequencePersister _changeSequencePersister;
        private readonly IOptions<LearningStandardsSynchronizationOptions> _defaultOptions;

        private readonly ILogger<LearningStandardsSynchronizer> _logger;

        private readonly IEdFiOdsApiAuthTokenManagerFactory _odsApiAuthTokenManagerFactory;

        private readonly IEdFiOdsApiClientConfiguration _odsApiClientConfiguration;

        public LearningStandardsSynchronizer(
            IEdFiOdsApiClientConfiguration odsApiClientConfiguration,
            IEdFiOdsApiAuthTokenManagerFactory odsApiAuthTokenManagerFactory,
            IEdFiBulkJsonPersisterFactory bulkJsonPersisterFactory,
            ILearningStandardsDataRetriever learningStandardsDataRetriever,
            ILearningStandardsProviderAuthTokenManagerFactory learningStandardsProviderAuthTokenManagerFactory,
            IChangeSequencePersister changeSequencePersister,
            IOptions<LearningStandardsSynchronizationOptions> defaultOptions,
            ILogger<LearningStandardsSynchronizer> logger)
        {
            _odsApiClientConfiguration = odsApiClientConfiguration;
            _odsApiAuthTokenManagerFactory = odsApiAuthTokenManagerFactory;
            _bulkJsonPersisterFactory = bulkJsonPersisterFactory;
            _learningStandardsDataRetriever = learningStandardsDataRetriever;
            _learningStandardsProviderAuthTokenManagerFactory = learningStandardsProviderAuthTokenManagerFactory;
            _changeSequencePersister = changeSequencePersister;
            _defaultOptions = defaultOptions;
            _logger = logger;

            _learningStandardsDataRetriever.ProcessCountEvent += OnCountEvent;
        }

        public async Task<IResponse> SynchronizeAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IAuthenticationConfiguration learningStandardsAuthenticationConfiguration,
            CancellationToken cancellationToken = default(CancellationToken),
            IProgress<LearningStandardsSynchronizerProgressInfo> progress = null)
        {
            Check.NotNull(odsApiConfiguration, nameof(odsApiConfiguration));
            Check.NotNull(learningStandardsAuthenticationConfiguration, nameof(learningStandardsAuthenticationConfiguration));

            return await SynchronizeAsync(
                    odsApiConfiguration,
                    learningStandardsAuthenticationConfiguration,
                    new LearningStandardsSynchronizationOptions { ForceFullSync = true },
                    cancellationToken,
                    progress)
                .ConfigureAwait(false);
        }

        public async Task<IResponse> SynchronizeAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IAuthenticationConfiguration learningStandardsAuthenticationConfiguration,
            ILearningStandardsSynchronizationOptions options,
            CancellationToken cancellationToken = default(CancellationToken),
            IProgress<LearningStandardsSynchronizerProgressInfo> progress = null)
        {
            Check.NotNull(odsApiConfiguration, nameof(odsApiConfiguration));
            Check.NotNull(learningStandardsAuthenticationConfiguration, nameof(learningStandardsAuthenticationConfiguration));
            Check.NotNull(options, nameof(options));

            try
            {
                if (options == null)
                {
                    options = _defaultOptions.Value;
                }

                var syncStartSequence = options.ForceFullSync
                    ? new ChangeSequence
                      {
                          Key = new ChangeSequenceKey(
                              odsApiConfiguration.OAuthAuthenticationConfiguration.Key,
                              learningStandardsAuthenticationConfiguration.Key)
                      }
                    : await _changeSequencePersister.GetAsync(
                        odsApiConfiguration.OAuthAuthenticationConfiguration.Key,
                        learningStandardsAuthenticationConfiguration.Key,
                        cancellationToken);

                var bulkJsonPersister = _bulkJsonPersisterFactory.CreateEdFiBulkJsonPersister(
                    _odsApiAuthTokenManagerFactory.CreateEdFiOdsApiAuthTokenManager(odsApiConfiguration),
                    odsApiConfiguration);

                var learningStandardAuthTokenManager =
                    _learningStandardsProviderAuthTokenManagerFactory
                        .CreateLearningStandardsProviderAuthTokenManager(
                            learningStandardsAuthenticationConfiguration);

                var descriptorResult = await EnsureDescriptors(
                        odsApiConfiguration,
                        bulkJsonPersister,
                        learningStandardAuthTokenManager,
                        syncStartSequence,
                        cancellationToken,
                        progress)
                    .ConfigureAwait(false);

                if (!descriptorResult.IsSuccess)
                {
                    return descriptorResult;
                }


                _logger.LogInformation("Retrieving available learning standard information.");

                var maxAvailableChangeSequence =
                    (await _learningStandardsDataRetriever.GetChangesAsync(
                                                              syncStartSequence,
                                                              learningStandardAuthTokenManager,
                                                              cancellationToken)
                                                          .ConfigureAwait(false)).ChangesAvailableInformation?.MaxAvailable;

                var syncResult = await EnsureLearningStandards(
                        odsApiConfiguration,
                        bulkJsonPersister,
                        learningStandardAuthTokenManager,
                        syncStartSequence,
                        cancellationToken,
                        progress)
                    .ConfigureAwait(false);

                if (syncResult.IsSuccess)
                {
                    if (maxAvailableChangeSequence != null)
                    {
                        await _changeSequencePersister.SaveAsync(
                            maxAvailableChangeSequence,
                            cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("The highest available sequence was not available for this sync, the current change status is unknown and thus will not be retained.");
                    }
                }

                return syncResult;
            }
            catch (Exception ex)
            {
                return ex.ToLearningStandardsResponse();
            }
        }

        public async Task<IChangesAvailableResponse> ChangesAvailableAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IAuthenticationConfiguration learningStandardsAuthenticationConfiguration,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(odsApiConfiguration, nameof(odsApiConfiguration));
            Check.NotNull(learningStandardsAuthenticationConfiguration, nameof(learningStandardsAuthenticationConfiguration));

            try
            {
                var learningStandardAuthTokenManager =
                    _learningStandardsProviderAuthTokenManagerFactory
                        .CreateLearningStandardsProviderAuthTokenManager(
                            learningStandardsAuthenticationConfiguration);

                if (_changeSequencePersister is DefaultChangeSequencePersister)
                {
                    _logger.LogWarning("Default persister in use. Change availability detection will always be true.");
                }

                var currentSequence = await _changeSequencePersister.GetAsync(
                    odsApiConfiguration.OAuthAuthenticationConfiguration.Key,
                    learningStandardsAuthenticationConfiguration.Key,
                    cancellationToken);

                return await _learningStandardsDataRetriever.GetChangesAsync(
                    currentSequence,
                    learningStandardAuthTokenManager,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                return new ChangesAvailableResponse(ex.ToLearningStandardsResponse(), null);
            }
        }

        private async Task<IResponse> EnsureDescriptors(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IEdFiBulkJsonPersister bulkJsonPersister,
            IAuthTokenManager learningAuthTokenManager,
            IChangeSequence syncStartSequence,
            CancellationToken cancellationToken,
            IProgress<LearningStandardsSynchronizerProgressInfo> progressEventHandler)
        {
            var results = new ConcurrentBag<IEnumerable<IResponse>>();

            progressEventHandler?.Report(new LearningStandardsSynchronizerProgressInfo(nameof(EnsureDescriptors), "Starting", DescriptorBaseProgressPercentage));
            int recordCounter = 0;

            try
            {
                await _learningStandardsDataRetriever
                    .GetLearningStandardsDescriptors(
                        odsApiConfiguration.Version,
                        syncStartSequence,
                        learningAuthTokenManager,
                        cancellationToken)
                    .AsyncEntityEnumerable
                    .ParallelForEachAsync(
                        async model =>
                        {
                            var result = await bulkJsonPersister
                                .PostEdFiBulkJson(model, cancellationToken)
                                .ConfigureAwait(false);

                            results.Add(result);
                            int currentRecordCount = Interlocked.Add(ref recordCounter, result.Count);

                            _logger.LogInformation(
                                "{TaskName} : {Status} : Cumulative records processed: {count}",
                                nameof(EnsureDescriptors),
                                "In Progress", currentRecordCount.ToString());

                            // For now, if the max is set at 1%, there's no need to display progress until this process
                            // is finished. This changes if we decide to display decimal progress, or can obtain the total
                            // record count.

                            //int constrainedProgressCounter =
                            //    sectionNumber + DescriptorBaseProgressPercentage >= DescriptorLimitProgressPercentage
                            //        ? DescriptorLimitProgressPercentage
                            //        : sectionNumber + DescriptorBaseProgressPercentage;

                            //progressEventHandler?.Report(
                            //    new LearningStandardsSynchronizerProgressInfo(
                            //        nameof(EnsureDescriptors),
                            //        "In Progress",
                            //        constrainedProgressCounter));
                        },
                        _odsApiClientConfiguration.MaxSimultaneousRequests,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (LearningStandardsHttpRequestException lsrEx)
            {
                if (lsrEx.HttpStatusCode == HttpStatusCode.Unauthorized || lsrEx.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogError(lsrEx, "Cannot start synchronization.");
                }

                return lsrEx.ToLearningStandardsResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured during descriptor synchronization.");
                return ex.ToLearningStandardsResponse();
            }

            progressEventHandler?.Report(new LearningStandardsSynchronizerProgressInfo(nameof(EnsureDescriptors), "Completed", DescriptorMaxProgressPercentage));

            return ResponseModel.Aggregate(results.SelectMany(sm => sm));
        }

        private async Task<IResponse> EnsureLearningStandards(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IEdFiBulkJsonPersister bulkJsonPersister,
            IAuthTokenManager learningAuthTokenManager,
            IChangeSequence syncStartSequence,
            CancellationToken cancellationToken,
            IProgress<LearningStandardsSynchronizerProgressInfo> progressEventHandler)
        {
            var processId = default(Guid);
            var results = new ConcurrentBag<IEnumerable<IResponse>>();

            progressEventHandler?.Report(new LearningStandardsSynchronizerProgressInfo(nameof(EnsureLearningStandards), "Starting", LearningStandardsBaseProgressPercentage));

            int recordCounter = 0;

            try
            {
                _logger.LogInformation("Synchronization process starting.");

                var learningStandardsProcess = _learningStandardsDataRetriever.GetLearningStandards(
                    odsApiConfiguration.Version,
                    syncStartSequence,
                    learningAuthTokenManager,
                    cancellationToken);

                processId = learningStandardsProcess.ProcessId;

                await learningStandardsProcess.AsyncEntityEnumerable
                    .ParallelForEachAsync(
                        async model =>
                        {
                            var result = await bulkJsonPersister
                                .PostEdFiBulkJson(
                                    model,
                                    cancellationToken)
                                .ConfigureAwait(false);

                            results.Add(result);
                            int currentRecordCount = Interlocked.Add(
                                ref recordCounter,
                                result.Count);

                            _logger.LogInformation(
                                "{TaskName} : {Status} : Cumulative records processed: {count}",
                                nameof(EnsureLearningStandards),
                                "In Progress",
                                currentRecordCount);

                            int totalRecordCount;
                            while (!_countsByProcessId.TryGetValue(processId, out totalRecordCount))
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            _logger.LogDebug($"TotalRecordCount: {totalRecordCount}");

                            // Takes the remaining percentage left after the descriptor process, determines progress, then
                            // adds back the base percentage.
                            int progressPercentage = Convert.ToInt32(
                                ((LearningStandardRemainingSyncProcessPercentage / (double) totalRecordCount)
                                * currentRecordCount) + LearningStandardsBaseProgressPercentage);


                            //Allows "Completed" status line to write the full 100%
                            progressPercentage = progressPercentage >= LearningStandardsLimitProgressPercentage
                                ? LearningStandardsLimitProgressPercentage
                                : progressPercentage;

                            _logger.LogDebug($"Current progress percentage: {progressPercentage}");

                            progressEventHandler?.Report(
                                new
                                    LearningStandardsSynchronizerProgressInfo(
                                        nameof(EnsureLearningStandards),
                                        "In Progress",
                                        progressPercentage));
                        },
                        _odsApiClientConfiguration.MaxSimultaneousRequests,
                        cancellationToken)
                    .ConfigureAwait(false);

                int expectedRecordCount;
                while (!_countsByProcessId.TryGetValue(processId, out expectedRecordCount))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (recordCounter < expectedRecordCount)
                {
                    string errorMessage = $"Not all expected records ({expectedRecordCount}) were processed ({recordCounter}).";
                    results.Add(
                        new IResponse[]
                        {
                            new ResponseModel(
                                false,
                                errorMessage,
                                null,
                                HttpStatusCode.InternalServerError)
                        });
                    _logger.LogError(errorMessage);
                }
            }
            catch (LearningStandardsHttpRequestException lsrEx)
            {
                if (lsrEx.HttpStatusCode == HttpStatusCode.Unauthorized
                    || lsrEx.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogError(lsrEx, "Cannot start synchronization.");
                }

                return lsrEx.ToLearningStandardsResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured during learning standards synchronization.");
                return ex.ToLearningStandardsResponse();
            }
            finally
            {
                _countsByProcessId.TryRemove(processId, out int _);
            }

            progressEventHandler?.Report(new LearningStandardsSynchronizerProgressInfo(nameof(EnsureLearningStandards), "Completed", LearningStandardsMaxProgressPercentage));

            _logger.LogInformation("Synchronization process complete.");

            return ResponseModel.Aggregate(results.SelectMany(sm => sm));
        }

        private void OnCountEvent(object o, AsyncEnumerableOperationStatus e)
        {
            _countsByProcessId.AddOrUpdate(e.ProcessId, e.TotalCount, (g, i) => e.TotalCount);
        }
    }
}
