// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using EdFi.Admin.LearningStandards.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EdFi.Admin.LearningStandards.Core.Auth
{
    public class AcademicBenchmarksAuthTokenManagerFactory : ILearningStandardsProviderAuthTokenManagerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AcademicBenchmarksAuthTokenManager> _logger;

        public AcademicBenchmarksAuthTokenManagerFactory(
            IServiceProvider serviceProvider,
            ILogger<AcademicBenchmarksAuthTokenManager> logger)
        {
            Check.NotNull(logger, nameof(logger));

            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IAuthTokenManager CreateLearningStandardsProviderAuthTokenManager(
            IAuthenticationConfiguration authenticationConfiguration)
        {
            Check.NotNull(authenticationConfiguration, nameof(authenticationConfiguration));

            return new AcademicBenchmarksAuthTokenManager(
                _serviceProvider.GetRequiredService<IOptionsSnapshot<AcademicBenchmarksOptions>>(),
                authenticationConfiguration,
                _logger);
        }
    }
}
