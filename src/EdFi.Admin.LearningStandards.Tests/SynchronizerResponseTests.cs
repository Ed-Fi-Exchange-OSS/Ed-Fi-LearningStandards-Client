// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using EdFi.Admin.LearningStandards.Core;
using NUnit.Framework;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace EdFi.Admin.LearningStandards.Tests
{
    [TestFixture]
    public class SynchronizerResponseTests
    {
        [Test]
        public void Can_aggregate_single_response()
        {
            //Arrange
            bool expectedSuccess = true;
            string expectedError = string.Empty;
            string expectedContent = "test";
            var expectedStatus = HttpStatusCode.OK;

            var responseList = new List<ResponseModel>
            {
                new ResponseModel(expectedSuccess, expectedError, expectedContent, expectedStatus)
            };

            //Act
            var actual = ResponseModel.Aggregate(responseList);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expectedContent, actual.Content);
            Assert.AreEqual(expectedError, actual.ErrorMessage);
            Assert.AreEqual(expectedStatus, actual.StatusCode);
            Assert.AreEqual(expectedSuccess, actual.IsSuccess);
        }

        [Test]
        public void Can_aggregate_multiple_success_responses()
        {
            //Arrange
            bool expectedSuccess = true;
            string expectedError = string.Empty;
            string responseContent = "test";
            string expectedContent = "test";
            var expectedStatus = HttpStatusCode.OK;

            var responseList = new List<ResponseModel>
            {
                new ResponseModel(expectedSuccess, expectedError, responseContent, expectedStatus),
                new ResponseModel(expectedSuccess, expectedError, responseContent, expectedStatus),
                new ResponseModel(expectedSuccess, expectedError, responseContent, expectedStatus)
            };

            //Act
            var actual = ResponseModel.Aggregate(responseList);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expectedContent, actual.Content);
            Assert.AreEqual(expectedError, actual.ErrorMessage);
            Assert.AreEqual(expectedStatus, actual.StatusCode);
            Assert.AreEqual(expectedSuccess, actual.IsSuccess);
        }

        [Test]
        public void Can_aggregate_multiple_failure_responses()
        {
            //Arrange
            bool expectedSuccess = false;
            string responseContent = "content";
            string expectedContent = $"{responseContent}";
            string responseError = "error";
            string expectedError = $"{responseError}";
            var expectedStatus = HttpStatusCode.OK;

            var responseList = new List<ResponseModel>
            {
                new ResponseModel(expectedSuccess, responseError, responseContent, expectedStatus),
                new ResponseModel(expectedSuccess, responseError, responseContent, expectedStatus),
                new ResponseModel(expectedSuccess, responseError, responseContent, expectedStatus)
            };

            //Act
            var actual = ResponseModel.Aggregate(responseList);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expectedContent, actual.Content);
            Assert.AreEqual(expectedError, actual.ErrorMessage);
            Assert.AreEqual(expectedStatus, actual.StatusCode);
            Assert.AreEqual(expectedSuccess, actual.IsSuccess);
        }

        [Test]
        public void Can_aggregate_mixed_failure_responses()
        {
            //Arrange
            bool expectedSuccess = false;
            string responseContent = "content";
            string expectedContent = $"{responseContent}";
            string responseError = "error";
            string expectedError = $"{responseError}";
            var expectedStatus = HttpStatusCode.Unauthorized;

            var responseList = new List<ResponseModel>
            {
                new ResponseModel(true, string.Empty, responseContent, HttpStatusCode.OK),
                new ResponseModel(expectedSuccess, responseError, responseContent, expectedStatus),
                new ResponseModel(true, string.Empty, responseContent, HttpStatusCode.NoContent)
            };

            //Act
            var actual = ResponseModel.Aggregate(responseList);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(3, actual.InnerResponses.Count);
            Assert.AreEqual(expectedContent, actual.Content);
            Assert.AreEqual(expectedError, actual.ErrorMessage);
            Assert.AreEqual(expectedStatus, actual.StatusCode);
            Assert.AreEqual(expectedSuccess, actual.IsSuccess);

            Console.Write(((ResponseModel)actual).ToString());
        }

        [Test]
        public void Can_process_single_exception()
        {
            //Arrange
            bool expectedSuccess = false;
            string expectedError = "The current credentials could not be validated.";
            string expectedContent = "{\"message\": \"Invalid token\"}";
            var expectedStatus = HttpStatusCode.Unauthorized;

            Exception httpException = new LearningStandardsHttpRequestException(expectedError, expectedStatus, expectedContent, ServiceNames.None);

            //Act
            var actual = httpException.ToLearningStandardsResponse();

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expectedContent, actual.Content);
            Assert.AreEqual(expectedError, actual.ErrorMessage);
            Assert.AreEqual(expectedStatus, actual.StatusCode);
            Assert.AreEqual(expectedSuccess, actual.IsSuccess);

            WriteResponse(actual);
        }

        [Test]
        public void Can_process_aggregate_exception()
        {
            //Arrange
            bool expectedSuccess = false;
            string expectedError = "The current credentials could not be validated.";
            string expectedContent = "{\"message\": \"Invalid token\"}";
            var expectedStatus = HttpStatusCode.Unauthorized;

            var httpException = new LearningStandardsHttpRequestException(expectedError, expectedStatus, expectedContent, ServiceNames.None);

            var argumentException = new ArgumentException("There was a problem getting your results.", "arg");

            var aggregateException = new AggregateException(httpException, argumentException);


            //Act
            var actual = aggregateException.ToLearningStandardsResponse();

            //Assert
            Assert.NotNull(actual);
            Assert.IsTrue(actual.Content.StartsWith(expectedContent));
            Assert.IsTrue(actual.ErrorMessage.StartsWith(expectedError));
            Assert.AreEqual(expectedStatus, actual.StatusCode);
            Assert.AreEqual(expectedSuccess, actual.IsSuccess);

            WriteResponse(actual);
        }

        private void WriteResponse(IResponse response)
        {
            Console.WriteLine($"{nameof(response.IsSuccess)}: {response.IsSuccess}");
            Console.WriteLine($"{nameof(response.StatusCode)}: {response.StatusCode}");
            Console.WriteLine($"{nameof(response.Content)}:\r\n{response.Content}");
            Console.WriteLine($"{nameof(response.ErrorMessage)}:\r\n{response.ErrorMessage}");
        }
    }
}
