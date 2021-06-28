// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Net.Http;
using EdFi.Admin.LearningStandards.Core.Configuration;
using Microsoft.Extensions.Logging;

namespace EdFi.Admin.LearningStandards.Core.Auth
{
    public class EdFiOdsApiAuthTokenManagerFactory
        : IEdFiOdsApiAuthTokenManagerFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerFactory _loggerFactory;

        public EdFiOdsApiAuthTokenManagerFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;
        }

        public IAuthTokenManager CreateEdFiOdsApiAuthTokenManager(IEdFiOdsApiConfiguration edFiOdsApiConfiguration)
        {
            switch (edFiOdsApiConfiguration.Version)
            {
                case EdFiOdsApiCompatibilityVersion.v2:
                    return new EdFiOdsApiv2AuthTokenManager(
                        edFiOdsApiConfiguration,
                        _httpClientFactory.CreateClient(nameof(IAuthTokenManager)),
                        _loggerFactory.CreateLogger<EdFiOdsApiv2AuthTokenManager>());
                case EdFiOdsApiCompatibilityVersion.v3:
                    return new EdFiOdsApiv3AuthTokenManager(
                        edFiOdsApiConfiguration,
                        _httpClientFactory.CreateClient(nameof(IAuthTokenManager)),
                        _loggerFactory.CreateLogger<EdFiOdsApiv3AuthTokenManager>());
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(edFiOdsApiConfiguration.Version),
                        edFiOdsApiConfiguration.Version,
                        null);
            }
        }
    }
}
