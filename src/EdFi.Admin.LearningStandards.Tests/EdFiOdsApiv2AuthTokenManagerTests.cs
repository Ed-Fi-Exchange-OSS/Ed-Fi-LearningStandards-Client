// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Auth;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Polly;

namespace EdFi.Admin.LearningStandards.Tests
{
    [TestFixture]
    public class EdFiOdsApiv2AuthTokenManagerTests
    {
        private const string _defaultOdsUrl = "https://api.testing-ed-fi.org/v2.5.0/api";

        private const string _expectedAccessToken = "940934d3a405492c99572db9329fc081";

        private const string _expectedAuthCode = "5a531101727f4ac7b1494bb9dea544a9";

        private const string _oAuthKey = "1a2b3c4d5e6f7g8h9i0j";

        private const string _oAuthSecret = "j0i9h8g7f6e5d4c3b2a1";

        private ILoggerFactory _loggerFactory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var lf = new Mock<ILoggerFactory>();
            lf.Setup(m => m.CreateLogger(It.IsAny<string>()))
                .Returns(new NUnitConsoleLogger<EdFiOdsApiv2AuthTokenManagerTests>());
            _loggerFactory = lf.Object;
        }

        [Test]
        public async Task Token_manager_can_retrieve_access_token()
        {
            //Arrange
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration(_oAuthKey, _oAuthSecret);
            IEdFiOdsApiConfiguration odsApiConfig = new EdFiOdsApiConfiguration(_defaultOdsUrl, EdFiOdsApiCompatibilityVersion.v2, authConfig);
            var logger = _loggerFactory.CreateLogger<EdFiOdsApiv2AuthTokenManager>();
            var httpHandler = new MockJsonHttpMessageHandler()
                .AddRouteResponse("authorize", GetDefaultAuthorizationResponse())
                .AddRouteResponse("token", GetDefaultAccessCodeResponse(_expectedAccessToken));
            var httpClient = GetConfiguredClient(httpHandler);

            var manager = new EdFiOdsApiv2AuthTokenManager(odsApiConfig, httpClient, logger);

            //Act
            string actual = await manager.GetTokenAsync().ConfigureAwait(false);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(_expectedAccessToken, actual);

            //2 API calls are made for the initial token: 1st being the authorize call, 2nd being the token call.
            Assert.AreEqual(2, httpHandler.CallCount);
        }

        [Test]
        public async Task Token_manager_can_retrieve_access_token_after_expiration()
        {
            //Arrange
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration(_oAuthKey, _oAuthSecret);
            IEdFiOdsApiConfiguration odsApiConfig = new EdFiOdsApiConfiguration(_defaultOdsUrl, EdFiOdsApiCompatibilityVersion.v2, authConfig);
            var logger = _loggerFactory.CreateLogger<EdFiOdsApiv2AuthTokenManager>();
            var httpHandler = new MockJsonHttpMessageHandler()
                .AddRouteResponse("authorize", GetDefaultAuthorizationResponse())
                .AddRouteResponse("token", GetDefaultAccessCodeResponse(expiresIn: 2))
                .AddRouteResponse("token", GetDefaultAccessCodeResponse(expiresIn: 2));
            var httpClient = GetConfiguredClient(httpHandler);

            var manager = new EdFiOdsApiv2AuthTokenManager(odsApiConfig, httpClient, logger);

            //Act
            string firstActual = await manager.GetTokenAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
            string secondActual = await manager.GetTokenAsync().ConfigureAwait(false);

            //Assert
            Assert.NotNull(firstActual);
            Assert.NotNull(secondActual);
            Assert.AreNotEqual(firstActual, secondActual);
            Assert.AreEqual(4, httpHandler.CallCount);
        }

        [Test]
        public async Task Token_manager_will_retry_after_failed_attempt()
        {
            //Arrange
            var apiDelay = TimeSpan.FromSeconds(1);

            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration(_oAuthKey, _oAuthSecret);
            IEdFiOdsApiConfiguration odsApiConfig = new EdFiOdsApiConfiguration(_defaultOdsUrl, EdFiOdsApiCompatibilityVersion.v2, authConfig);
            var logger = _loggerFactory.CreateLogger<EdFiOdsApiv2AuthTokenManager>();
            var httpHandler = new MockJsonHttpMessageHandler()
                .AddRouteResponse("authorize", HttpStatusCode.InternalServerError, new JObject(), apiDelay)
                .AddRouteResponse("authorize", GetDefaultAuthorizationResponse())
                .AddRouteResponse("token", GetDefaultAccessCodeResponse());
            var httpClient = GetConfiguredClient(httpHandler);

            var manager = new EdFiOdsApiv2AuthTokenManager(odsApiConfig, httpClient, logger);

            //Act
            string firstActual = await manager.GetTokenAsync().ConfigureAwait(false);

            //Assert
            Assert.NotNull(firstActual);
            Assert.AreEqual(3, httpHandler.CallCount);
        }

        private HttpClient GetConfiguredClient(HttpMessageHandler handler, Func<PolicyBuilder<HttpResponseMessage>, IAsyncPolicy<HttpResponseMessage>> configurePolicy = null)
        {
            var serviceProvider = new ServiceCollection()
                .AddHttpClient(nameof(EdFiOdsApiv2AuthTokenManagerTests))
                .ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddTransientHttpErrorPolicy(p =>
                    configurePolicy?.Invoke(p) ?? p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)))
                .Services.BuildServiceProvider();

            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient(nameof(EdFiOdsApiv2AuthTokenManagerTests));
        }

        private JObject GetDefaultAuthorizationResponse()
        {
            return new JObject { ["code"] = _expectedAuthCode };
        }

        private JObject GetDefaultAccessCodeResponse(string accessCode = null, int expiresIn = 30)
        {
            return new JObject
            {
                ["access_token"] = accessCode ?? Guid.NewGuid().ToString("N"),
                ["expires_in"] = expiresIn,
                ["token_type"] = "bearer"
            };
        }
    }
}
