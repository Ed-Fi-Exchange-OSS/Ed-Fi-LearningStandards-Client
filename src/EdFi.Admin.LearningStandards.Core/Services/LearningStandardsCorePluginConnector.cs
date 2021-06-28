// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Installers;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EdFi.Admin.LearningStandards.Core.Services
{
    public class LearningStandardsCorePluginConnector
        : ILearningStandardsCorePluginConnector
    {
        public LearningStandardsCorePluginConnector(
            IServiceCollection serviceCollection,
            Func<IServiceCollection, IServiceProvider> getServiceProvider,
            ILoggerProvider loggerProvider,
            IEdFiOdsApiClientConfiguration odsApiClientConfiguration)
        {
            serviceCollection.AddLearningStandardsServices(odsApiClientConfiguration ?? new EdFiOdsApiClientConfiguration());

            //Todo: Confirm that the AddLogging extension method checks for existing logging resources before adding them.
            serviceCollection.AddLogging(lb => lb.AddProvider(loggerProvider));

            var serviceProvider = getServiceProvider.Invoke(serviceCollection);

            LearningStandardsSynchronizer = serviceProvider.GetRequiredService<ILearningStandardsSynchronizer>();
            LearningStandardsConfigurationValidator = serviceProvider.GetRequiredService<ILearningStandardsConfigurationValidator>();
            LearningStandardsChangesAvailable = serviceProvider.GetRequiredService<ILearningStandardsChangesAvailable>();
        }

        public ILearningStandardsSynchronizer LearningStandardsSynchronizer { get; }

        public ILearningStandardsConfigurationValidator LearningStandardsConfigurationValidator { get; }

        public ILearningStandardsChangesAvailable LearningStandardsChangesAvailable { get; }
    }
}
