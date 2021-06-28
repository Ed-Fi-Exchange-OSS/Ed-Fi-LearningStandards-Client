// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Auth;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests
{
    [TestFixture]
    public class AcademicBenchmarksAuthTokenManagerTests
    {
        private ILogger<AcademicBenchmarksAuthTokenManager> _logger;

        private IServiceProvider _serviceProvider;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _logger = new NUnitConsoleLogger<AcademicBenchmarksAuthTokenManager>();

            var academicBenchmarksSnapshotOption = new Mock<IOptionsSnapshot<AcademicBenchmarksOptions>>();
            academicBenchmarksSnapshotOption.Setup(x => x.Value)
                .Returns(new AcademicBenchmarksOptions());

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IOptionsSnapshot<AcademicBenchmarksOptions>)))
                .Returns(academicBenchmarksSnapshotOption.Object);

            _serviceProvider = serviceProvider.Object;
        }

        [Test]
        public async Task Get_token_async_can_be_decoded()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);
            var decoded = DecodeToken(result);

            //Assert
            Assert.IsNotNull(decoded);
        }

        [Test]
        public async Task Get_token_async_can_find_cached_value()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);
            string cachedResult = await tokenManager.GetTokenAsync().ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.AreEqual(result, cachedResult);
        }

        [Test]
        public async Task Get_token_async_generates_expires_value()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);
            var decoded = DecodeToken(result);

            //Assert
            Assert.IsNotNull(decoded);
            Assert.IsNotNull(decoded.Value<JObject>("auth").Value<string>("auth.expires"));
        }

        [Test]
        public async Task Get_token_async_generates_proper_expires_window()
        {
            //Arrange
            int tokenLengthHours = 24;
            int varianceBufferSeconds = 1;
            long s = new DateTimeOffset(DateTime.UtcNow.AddHours(tokenLengthHours)).ToUnixTimeSeconds();
            long lower = s - varianceBufferSeconds;
            long upper = s + varianceBufferSeconds;

            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);
            var decoded = DecodeToken(result);
            string expires = decoded.Value<JObject>("auth").Value<string>("auth.expires");
            long actual = long.Parse(expires);

            //Assert
            Assert.IsNotNull(decoded);
            Assert.IsNotNull(expires);
            Assert.That(actual, Is.InRange(lower, upper));
        }

        [Test]
        public async Task Get_token_async_generates_proper_signature_length()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);
            var decoded = DecodeToken(result);
            string sig = decoded.Value<JObject>("auth").Value<string>("auth.signature");
            var sigBytes = Convert.FromBase64String(sig);

            //Assert
            Assert.AreEqual(256, sigBytes.Length * 8);
        }

        [Test]
        public async Task Get_token_async_generates_proper_token_structure()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);
            var decoded = DecodeToken(result);
            var payload = decoded.Value<JObject>("auth");

            //Assert
            Assert.IsNotNull(decoded);
            Assert.IsTrue(decoded.Properties().Any(an => an.Name == "auth"));
            Assert.IsNotNull(payload);
            Assert.IsTrue(payload.Properties().Any(an => an.Name == "partner.id"));
            Assert.IsTrue(payload.Properties().Any(an => an.Name == "auth.expires"));
            Assert.IsTrue(payload.Properties().Any(an => an.Name == "auth.signature"));
        }

        [Test]
        public async Task Get_token_async_generates_signature_value()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);
            var decoded = DecodeToken(result);

            //Assert
            Assert.IsNotNull(decoded);
            Assert.IsNotNull(decoded.Value<JObject>("auth").Value<string>("auth.signature"));
        }

        [Test]
        public async Task Get_token_async_generates_value()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public async Task Get_token_async_returns_proper_partner_id()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Act
            string result = await tokenManager.GetTokenAsync().ConfigureAwait(false);
            var decoded = DecodeToken(result);

            //Assert
            Assert.IsNotNull(decoded);
            Assert.IsNotNull(decoded.Value<JObject>("auth").Value<string>("partner.id"));
            Assert.AreEqual("123", decoded.Value<JObject>("auth").Value<string>("partner.id"));
        }

        [Test]
        public void Token_manager_factory_generates_correct_type()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);
            IAuthenticationConfiguration authConfig = new AuthenticationConfiguration("123", "456");

            //Act
            var tokenManager = factory.CreateLearningStandardsProviderAuthTokenManager(authConfig);

            //Assert
            Assert.IsNotNull(tokenManager);
            Assert.AreEqual(typeof(AcademicBenchmarksAuthTokenManager), tokenManager.GetType());
        }

        [Test]
        public void Token_manager_factory_throws_on_null_logger()
        {
            //Act -> Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                var factory = new AcademicBenchmarksAuthTokenManagerFactory(null, null);
            });
        }

        [Test]
        public void Token_manager_throws_on_create_with_null_options()
        {
            //Arrange
            var factory = new AcademicBenchmarksAuthTokenManagerFactory(_serviceProvider, _logger);

            //Act -> Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                factory.CreateLearningStandardsProviderAuthTokenManager(null);
            });
        }

        private static JObject DecodeToken(string token)
        {
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            return JObject.Parse(json);
        }
    }
}
