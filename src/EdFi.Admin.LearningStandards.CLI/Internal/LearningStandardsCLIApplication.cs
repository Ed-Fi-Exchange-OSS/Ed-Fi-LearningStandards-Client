// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using EdFi.Admin.LearningStandards.CLI.Internal;
using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Installers;
using EdFi.Admin.LearningStandards.Core.Services;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EdFi.Admin.LearningStandards.CLI
{
    public class LearningStandardsCLIApplication
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly Action<IServiceCollection> _serviceSetup;

        private LearningStandardsCLIBaseOptions _options = new LearningStandardsCLIBaseOptions();

        private ILearningStandardsSynchronizer _synchronizer;

        private ILearningStandardsConfigurationValidator _validator;

        private ILearningStandardsChangesAvailable _changesAvailable;

        public LearningStandardsCLIApplication()
        {
        }

        internal LearningStandardsCLIApplication(Action<IServiceCollection> serviceSetup)
        {
            _serviceSetup = serviceSetup;
        }

        internal LearningStandardsCLIWriter CliWriter { get; private set; }

        internal bool Unattended => _options.Unattended;

        internal async Task<IResponse> Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            //Parse arguments
            var parser = new Parser(
                config =>
                {
                    config.EnableDashDash = true;
                    config.HelpWriter = Console.Out;
                });
            int parseCode = parser
                            .ParseArguments<LearningStandardsCLISyncOptions,
                                LearningStandardsCLIValidateOptions,
                                LearningStandardsCLIChangesOptions>(args)
                            .MapResult(
                                (LearningStandardsCLISyncOptions syncOptions) =>
                                {
                                    _options = syncOptions;
                                    return 0;
                                },
                                (LearningStandardsCLIValidateOptions validateOptions) =>
                                {
                                    _options = validateOptions;
                                    return 0;
                                },
                                (LearningStandardsCLIChangesOptions changeOptions) =>
                                {
                                    _options = changeOptions;
                                    return 0;
                                },
                                errs => 1);

            if (parseCode > 0)
            {
                //Could not parse arguments. Let the default usage screen render.
                throw new LearningStandardsCLIParserException();
            }

            // Add and setup application services and configuration settings.
            SetupApplicationServices();

            //Validate both AB provider options and Ed-Fi ODS options
            var validationResults = await ValidateConfiguration().ConfigureAwait(false);

            //When validating, a success message will output to the console by default.
            //If there are validation errors, return
            if (!validationResults.IsSuccess)
            {
                CliWriter.Error(validationResults.ToString());
                return validationResults;
            }

            switch (_options)
            {
                //Validation summary only
                case LearningStandardsCLIValidateOptions _:
                    {
                        return validationResults;
                    }
                //Changes summary only
                case LearningStandardsCLIChangesOptions changesOptions:
                    {
                        return await RetrieveChangeSummary(changesOptions).ConfigureAwait(false);
                    }
                default:
                    {
                        return await ExecuteSync(_options as LearningStandardsCLISyncOptions).ConfigureAwait(false);
                    }
            }
        }


        internal async Task<IResponse> RetrieveChangeSummary(LearningStandardsCLIChangesOptions changesOptions)
        {
            var changesResult = await _changesAvailable.ChangesAvailableAsync(
                    changesOptions.ToEdFiOdsApiConfiguration(),
                    changesOptions.ToAcademicBenchmarksAuthenticationConfiguration(),
                    _cts.Token)
                .ConfigureAwait(false);

            if (changesOptions.OutputFormat.ToLower() == "json")
            {
                CliWriter.Json(changesResult);
            }
            else
            {
                if (!changesResult.IsSuccess)
                {
                    CliWriter.Error(
                        $"[{(int)changesResult.StatusCode}] {changesResult.ErrorMessage}");
                }
                else
                {
                    CliWriter.Info(changesResult.Content);
                }
            }

            return changesResult;
        }

        internal async Task<IResponse> ExecuteSync(LearningStandardsCLISyncOptions syncOptions)
        {
            _cts.Token.ThrowIfCancellationRequested();
            CliWriter.Info("Starting synchronization");
            var progress = new Progress<LearningStandardsSynchronizerProgressInfo>(Synchronizer_Progress);
            var options = new LearningStandardsSynchronizationOptions
                          {
                              ForceFullSync = syncOptions?.ForceFullSync ?? false
                          };

            var syncResponse = await _synchronizer.SynchronizeAsync(
                    // ReSharper disable once PossibleNullReferenceException
                    syncOptions.ToEdFiOdsApiConfiguration(),
                    syncOptions
                                                          .ToAcademicBenchmarksAuthenticationConfiguration(),
                                                      options,
                                                      _cts.Token,
                                                      progress)
                                                  .ConfigureAwait(false);

            if (syncResponse.IsSuccess)
            {
                CliWriter.Info("Synchronization complete.", syncResponse.Content);
            }
            else
            {
                CliWriter.Error($"[{(int) syncResponse.StatusCode}] {syncResponse.ErrorMessage}");
            }

            return syncResponse;
        }

        internal async Task<IResponse> ValidateConfiguration()
        {
            _cts.Token.ThrowIfCancellationRequested();
            CliWriter.Info("Validating configuration");

            var validationResponse = await _validator.ValidateConfigurationAsync(
                    _options.ToAcademicBenchmarksAuthenticationConfiguration(),
                    _options.ToEdFiOdsApiConfiguration())
                .ConfigureAwait(false);


            if (validationResponse.IsSuccess)
            {
                CliWriter.Info("Configuration options are valid", validationResponse.Content);
            }

            return validationResponse;
        }

        /// <summary>
        /// Responsible for creating and configuring the internal ServiceProvider.
        /// </summary>
        private void SetupApplicationServices()
        {
            CliWriter = new LearningStandardsCLIWriter(_options.Verbose);

            //Root service collection
            var services = new ServiceCollection();

            //Setup default console logger
            CliWriter.Verbose("Arguments parsed successfully");

            //Create additional logging services
            CliWriter.Verbose("Creating logging services");
            services.AddLogging(
                opts =>
                {
                    opts.AddLog4Net();
                    opts.SetMinimumLevel(LogLevel.Trace);
                    opts.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
                });

            //Add learning standards services
            CliWriter.Verbose("Adding learning standard services");
            services.AddLearningStandardsServices(_options.ToEdFiOdsApiClientConfiguration());

            services.Configure<AcademicBenchmarksOptions>(
                opts =>
                {
                    opts.AuthorizationWindowSeconds = _options.AcademicBenchmarksAuthorizationWindow;
                    opts.Retries = _options.AcademicBenchmarksRetryLimit;
                    if (!string.IsNullOrWhiteSpace(_options.AcademicBenchmarksProxyUrl))
                    {
                        opts.Url = _options.AcademicBenchmarksProxyUrl;
                    }
                });

            services.AddSingleton(x => (ILearningStandardsProviderConfiguration)x.GetService(typeof(AcademicBenchmarksOptions)));
            services.AddSingleton<IChangeSequencePersister, JsonFileChangeSequencePersister>();

            services.Configure<JsonFileChangeSequencePersisterOptions>(opts =>
            {
                opts.FileName = _options.ChangeSequenceStorePath;
            });

            _serviceSetup?.Invoke(services);
            var serviceProvider = services.BuildServiceProvider();
            _validator = serviceProvider.GetRequiredService<ILearningStandardsConfigurationValidator>();
            _synchronizer = serviceProvider.GetRequiredService<ILearningStandardsSynchronizer>();
            _changesAvailable = serviceProvider.GetRequiredService<ILearningStandardsChangesAvailable>();
        }

        private void Synchronizer_Progress(LearningStandardsSynchronizerProgressInfo e)
        {
            CliWriter.Info(
                $"{e.TaskName} is {e.TaskState} : Synchronization is {e.CompletedPercentage}% completed.");
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _cts.Cancel();
            e.Cancel = true;
        }
    }
}
