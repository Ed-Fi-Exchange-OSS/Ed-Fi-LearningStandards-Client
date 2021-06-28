// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.CLI.Utilities;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EdFi.Admin.LearningStandards.CLI.Internal
{
    public class JsonFileChangeSequencePersister : IChangeSequencePersister
    {
        private readonly ILogger<JsonFileChangeSequencePersister> _logger;
        private readonly JsonFileChangeSequencePersisterOptions _options;

        private readonly ConcurrentDictionary<IChangeSequenceKey, IChangeSequence> _store;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
                                                                      {
                                                                          Formatting = Formatting.Indented,
                                                                          ContractResolver =
                                                                              new DictionaryAsArrayResolver(),
                                                                          Converters =
                                                                          {
                                                                              new AbstractConverter<ChangeSequenceKey,IChangeSequenceKey>(),
                                                                              new AbstractConverter<ChangeSequence,IChangeSequence>()
                                                                          }
                                                                      };

        public JsonFileChangeSequencePersister(
            IOptions<JsonFileChangeSequencePersisterOptions> options,
            ILogger<JsonFileChangeSequencePersister> logger)
        {
            _logger = Check.NotNull(logger, nameof(logger));
            _options = Check.NotNull(options?.Value, nameof(options.Value));

            if (string.IsNullOrWhiteSpace(_options.FileName))
            {
                throw new ArgumentException(
                    $"The {nameof(JsonFileChangeSequencePersister)} requires a file name to be specified for persisting change sequences.",
                    nameof(_options.FileName));
            }

            string jsonText = null;

            try
            {
                if (File.Exists(_options.FileName))
                {
                    jsonText = File.ReadAllText(_options.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while reading file: {_options.FileName}");
                throw;
            }

            if (!string.IsNullOrWhiteSpace(jsonText))
            {
                try
                {
                    _store = new ConcurrentDictionary<IChangeSequenceKey, IChangeSequence>(
                        JsonConvert.DeserializeObject<Dictionary<IChangeSequenceKey, IChangeSequence>>(
                            File.ReadAllText(_options.FileName),
                            _serializerSettings));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while parsing json from file: {_options.FileName}.");
                    throw;
                }
            }
            else
            {
                _store = new ConcurrentDictionary<IChangeSequenceKey, IChangeSequence>();
            }
        }

        public async Task SaveAsync(
            IChangeSequence changeSequence,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(changeSequence, nameof(changeSequence));

            _store.AddOrUpdate(changeSequence.Key, changeSequence,
                (k, o) =>
                {
                    o.Id = changeSequence.Id;
                    return o;
                });

            await File.WriteAllTextAsync(
                _options.FileName,
                JsonConvert.SerializeObject(_store, _serializerSettings),
                cancellationToken);
        }

        public async Task<IChangeSequence> GetAsync(
            string edFiApiKey,
            string learningStandardCredentialId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value = new ChangeSequence
                        {
                            Id = default,
                            Key = new ChangeSequenceKey(edFiApiKey, learningStandardCredentialId)
                        };

            return await Task.FromResult(_store.GetOrAdd(value.Key, value))
                             .ConfigureAwait(false);
        }

    }
}
