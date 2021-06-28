// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Services;
using EdFi.Admin.LearningStandards.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests.IntegrationTests
{
    [TestFixture]
    public class LearningStandardsCorePluginConnectorIntegrationTests
    {
        [Test]
        public void Can_initialize_plugin()
        {
            // Arrange
            var edfiOdsApiClientConfiguration = new EdFiOdsApiClientConfiguration();
            var serviceCollection = new ServiceCollection();
            IServiceProvider ServiceProviderFunc(IServiceCollection collection) => collection.BuildServiceProvider();

            // Act
            var pluginConnector = new LearningStandardsCorePluginConnector(
                serviceCollection,
                ServiceProviderFunc,
                new NUnitLoggerProvider(),
                edfiOdsApiClientConfiguration
            );

            // Assert
            Assert.IsNotNull(pluginConnector);
            Assert.IsNotNull(pluginConnector.LearningStandardsConfigurationValidator);
            Assert.IsNotNull(pluginConnector.LearningStandardsSynchronizer);
            Assert.IsNotNull(pluginConnector.LearningStandardsChangesAvailable);
        }
    }
}
