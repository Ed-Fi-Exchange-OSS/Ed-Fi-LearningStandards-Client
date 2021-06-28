// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Auth;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Services;
using EdFi.Admin.LearningStandards.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests
{
    [TestFixture]
    public class EdFiBulkJsonPersisterTests
    {
        private ILogger<EdFiBulkJsonPersister> _logger;
        private EdFiOdsApiConfiguration _v2Configuration;
        private EdFiOdsApiConfiguration _v3Configuration;
        private Mock<IAuthTokenManager> _authTokenManager;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _logger = new NUnitConsoleLogger<EdFiBulkJsonPersister>();
            _v2Configuration = new EdFiOdsApiConfiguration(
                "http://localhost:7000",
                EdFiOdsApiCompatibilityVersion.v2,
                new AuthenticationConfiguration("key", "secret"),
                2018);
            _v3Configuration = new EdFiOdsApiConfiguration(
                "http://localhost:7000",
                EdFiOdsApiCompatibilityVersion.v3,
                new AuthenticationConfiguration("key", "secret"));

            _authTokenManager = new Mock<IAuthTokenManager>();
            _authTokenManager.Setup(x => x.GetTokenAsync())
                             .Returns(Task.FromResult("ThisIsAToken"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("Upsert")]
        public async Task Should_Process_Bulk_Json_Operations_Other_For_Upsert(string operation)
        {
            // Arrange
            var fakeHttpMessageHandler = new MockJsonHttpMessageHandler();
            var httpClient = new HttpClient(fakeHttpMessageHandler);

            var sut = new EdFiBulkJsonPersister(
                _v2Configuration,
                _authTokenManager.Object,
                _logger,
                httpClient);

            var edfiBulkJson = new EdFiBulkJsonModel
                               { Data = new List<JObject>(), Operation = operation, Resource = "Student" };

            // Act
            var result = await sut.PostEdFiBulkJson(edfiBulkJson).ConfigureAwait(false);

            //Assert
            Assert.IsEmpty(result);
        }

        [TestCase("Create")]
        [TestCase("Update")]
        [TestCase("Delete")]
        [TestCase("OtherValue")]
        public void Should_Not_Process_Bulk_Json_Operations_That_Are_Not_Upsert(string operation)
        {
            // Arrange

            var httpClient = new HttpClient(new MockJsonHttpMessageHandler());

            var sut = new EdFiBulkJsonPersister(
                _v2Configuration,
                _authTokenManager.Object,
                _logger,
                httpClient);

            var edfiBulkJson = new EdFiBulkJsonModel { Operation = operation, Resource = "Student" };

            // Act => Assert
            Assert.ThrowsAsync<NotSupportedException>(async() => await sut.PostEdFiBulkJson(edfiBulkJson).ConfigureAwait(false));
        }

        [TestCaseSource(typeof(FileBasedTestCases),nameof(FileBasedTestCases.ValidBulkJsonTestCases))]
        public async Task Should_Successfully_Process_Valid_Bulk_Json_Operations(EdFiOdsApiCompatibilityVersion version, EdFiBulkJsonModel testData, int expectedDataCount)
        {
            var config = version == EdFiOdsApiCompatibilityVersion.v2
                ? _v2Configuration
                : _v3Configuration;

            // Arrange
            var fakeHttpMessageHandler = new MockJsonHttpMessageHandler();
            fakeHttpMessageHandler.AddRouteResponse(
                EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(
                                               config.Url,
                                               testData.Schema,
                                               testData.Resource,
                                               config.Version,
                                               config.SchoolYear)
                                           .LocalPath,
                HttpStatusCode.Created);

            var httpClient = new HttpClient(fakeHttpMessageHandler);

            var sut = new EdFiBulkJsonPersister(
                config,
                _authTokenManager.Object,
                _logger,
                httpClient);

            // Act
            var result = (await sut.PostEdFiBulkJson(testData).ConfigureAwait(false)).ToList();

            //Assert
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.All(x=> x.IsSuccess));
            Assert.AreEqual(expectedDataCount, result.Count);
        }

        [TestCaseSource(typeof(FileBasedTestCases), nameof(FileBasedTestCases.TestDataOnlyValidBatchJson))]
        public async Task Should_Handle_Api_Errors(EdFiOdsApiCompatibilityVersion version, EdFiBulkJsonModel testData, int expectedDataCount)
        {
            var config = version == EdFiOdsApiCompatibilityVersion.v2
                ? _v2Configuration
                : _v3Configuration;

            // Arrange
            var fakeHttpMessageHandler = new MockJsonHttpMessageHandler();
            fakeHttpMessageHandler.AddRouteResponse(
                EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(
                                               config.Url,
                                               testData.Schema,
                                               testData.Resource,
                                               config.Version,
                                               config.SchoolYear)
                                           .LocalPath,
                HttpStatusCode.Conflict);

            var httpClient = new HttpClient(fakeHttpMessageHandler);

            var sut = new EdFiBulkJsonPersister(
                config,
                _authTokenManager.Object,
                _logger,
                httpClient);

            // Act
            var result = (await sut.PostEdFiBulkJson(testData).ConfigureAwait(false)).ToList();

            //Assert
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.All(x => !x.IsSuccess));
            Assert.AreEqual(expectedDataCount, result.Count);
        }

        private class FileBasedTestCases
        {
            private static readonly TestCaseData[] _validBulkJsonTestCases = new[]
                                          {
                                              new TestCaseData(
                                                  EdFiOdsApiCompatibilityVersion.v2,
                                                  JToken.Parse(
                                                            TestCaseHelper.GetTestCaseTextFromFile("Valid-Descriptors-v2.txt"))
                                                        .First.ToObject<EdFiBulkJsonModel>(),
                                                  8).SetName(
                                                  $"{Descriptors}-{EdFiOdsApiCompatibilityVersion.v2}-Success"),
                                              new TestCaseData(
                                                  EdFiOdsApiCompatibilityVersion.v3,
                                                  JToken.Parse(
                                                            TestCaseHelper.GetTestCaseTextFromFile("Valid-Descriptors-v3.txt"))
                                                        .Last
                                                        .ToObject<EdFiBulkJsonModel>(),
                                                  3).SetName(
                                                  $"{Descriptors}-{EdFiOdsApiCompatibilityVersion.v3}-Success"),
                                              new TestCaseData(
                                                  EdFiOdsApiCompatibilityVersion.v2,
                                                  JToken.Parse(
                                                            TestCaseHelper.GetTestCaseTextFromFile("Valid-Sync-v2.txt"))
                                                        .First
                                                        .ToObject<EdFiBulkJsonModel>(),
                                                  2).SetName(
                                                  $"{LearningStandards}-{EdFiOdsApiCompatibilityVersion.v2}-Success"),
                                              new TestCaseData(
                                                  EdFiOdsApiCompatibilityVersion.v3,
                                                  JToken.Parse(
                                                            TestCaseHelper.GetTestCaseTextFromFile("Valid-Sync-v3.txt"))
                                                        .First
                                                        .ToObject<EdFiBulkJsonModel>(),
                                                  2).SetName(
                                                  $"{LearningStandards}-{EdFiOdsApiCompatibilityVersion.v3}-Success"),
                                          };

            internal static object[] TestDataOnlyValidBatchJson()
            {
                return _validBulkJsonTestCases.Select(x => x.Arguments).ToArray();
            }
            internal static object[] ValidBulkJsonTestCases()
            {
                return _validBulkJsonTestCases;
            }

            private const string LearningStandards = "LearningStandards";

            private const string Descriptors = "Descriptors";
        }
    }
}
