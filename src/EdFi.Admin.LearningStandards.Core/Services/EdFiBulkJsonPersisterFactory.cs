// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net.Http;
using EdFi.Admin.LearningStandards.Core.Auth;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EdFi.Admin.LearningStandards.Core.Services
{
    public interface IEdFiBulkJsonPersisterFactory
    {
        IEdFiBulkJsonPersister CreateEdFiBulkJsonPersister(IAuthTokenManager authTokenManager, IEdFiOdsApiConfiguration odsApiConfiguration);
    }

    public class EdFiBulkJsonPersisterFactory : IEdFiBulkJsonPersisterFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EdFiBulkJsonPersister> _logger;

        public EdFiBulkJsonPersisterFactory(IHttpClientFactory httpClientFactory, ILogger<EdFiBulkJsonPersister> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IEdFiBulkJsonPersister CreateEdFiBulkJsonPersister(IAuthTokenManager authTokenManager, IEdFiOdsApiConfiguration odsApiConfiguration)
        {
            return new EdFiBulkJsonPersister(
                odsApiConfiguration,
                authTokenManager,
                _logger,
                _httpClientFactory.CreateClient(nameof(IEdFiBulkJsonPersister)));
        }
    }
}
