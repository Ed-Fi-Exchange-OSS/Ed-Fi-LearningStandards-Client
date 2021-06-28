// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace EdFi.Admin.LearningStandards.Core
{
    public class ResponseModel : IResponse
    {
        public ResponseModel(bool isSuccess, string errorMessage, string content, HttpStatusCode statusCode)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Content = content;
            StatusCode = statusCode;
        }

        public ResponseModel(bool isSuccess, string errorMessage, string content, HttpStatusCode statusCode, IEnumerable<IResponse> innerResponses)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Content = content;
            StatusCode = statusCode;
            InnerResponses = innerResponses.ToList();
        }

        public bool IsSuccess { get; }

        public string ErrorMessage { get; }

        public string Content { get; }

        public HttpStatusCode StatusCode { get; }

        public List<IResponse> InnerResponses { get; } = new List<IResponse>();

        public override string ToString()
        {
            return !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage : Content;
        }

        public static IResponse Aggregate(IEnumerable<IResponse> groupResponses)
        {
            var flattened = groupResponses.Flatten().ToList();
            if (flattened.Count == 0)
            {
                return ResponseModel.Success("There were no responses to aggregate from the specified operation.");
            }

            var candidate = flattened[0];
            if (flattened.Any(al => !al.IsSuccess))
            {
                candidate = flattened.OrderByDescending(ob => ob.StatusCode).First();
            }
            return new ResponseModel(candidate.IsSuccess, candidate.ErrorMessage, candidate.Content, candidate.StatusCode, flattened);
        }

        public static IResponse Error(string errorMessage, HttpStatusCode statusCode, string additionalContent = "")
        {
            return new ResponseModel(false, errorMessage, additionalContent, statusCode);
        }

        public static IResponse Success(string content = "Success", HttpStatusCode statusCode = HttpStatusCode.OK, string additionalContent = "")
        {
            return new ResponseModel(true, string.Empty, content, statusCode);
        }
    }
}
