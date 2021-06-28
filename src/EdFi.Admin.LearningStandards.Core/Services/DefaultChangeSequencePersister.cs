// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EdFi.Admin.LearningStandards.Core.Services
{
    /// <summary>
    /// Provides the ability to inject a default provider instead of null. Methods are passthrough.
    /// </summary>
    public class DefaultChangeSequencePersister : IChangeSequencePersister
    {
        private readonly ILogger<DefaultChangeSequencePersister> _logger;

        public DefaultChangeSequencePersister(ILogger<DefaultChangeSequencePersister> logger)
        {
            _logger = logger;
        }

        public Task SaveAsync(
            IChangeSequence changeSequence,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(changeSequence, nameof(changeSequence));

            _logger.LogDebug("Save method for DefaultChangeSequencePersister was called. Implement and add an IChangeSequencePersister to your service collection to enable change sequence persistence.");
            _logger.LogInformation($"Save was called for the DefaultChangeSequencePersister. The following change sequence id was NOT saved: {changeSequence.Id}") ;
            return Task.CompletedTask;
        }

        public Task<IChangeSequence> GetAsync(string edFiApiKey, string learningStandardCredentialId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotEmpty(edFiApiKey, nameof(edFiApiKey));
            Check.NotEmpty(learningStandardCredentialId, nameof(learningStandardCredentialId));

            _logger.LogDebug("Get method for DefaultChangeSequencePersister was called. Implement and add an IChangeSequencePersister to your service collection to enable change sequence persistence.");
            return Task.FromResult(
                new ChangeSequence { Key = new ChangeSequenceKey(edFiApiKey, learningStandardCredentialId) } as IChangeSequence);
        }
    }
}
