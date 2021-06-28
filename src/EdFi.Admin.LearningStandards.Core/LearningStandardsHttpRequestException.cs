// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Net;
using System.Net.Http;

namespace EdFi.Admin.LearningStandards.Core
{
    public class LearningStandardsHttpRequestException : HttpRequestException
    {
        public LearningStandardsHttpRequestException(string message, HttpStatusCode httpStatusCode, string responseContent, string serviceName)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
            ResponseContent = string.IsNullOrEmpty(responseContent) ? httpStatusCode.ToString() : responseContent;
            ServiceName = serviceName;
        }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string ResponseContent { get; set; }

        public string ServiceName { get; set; }
    }

    public static class ServiceNames
    {
        public const string AB = "Academic Benchmarks";

        public const string EdFi = "Ed-Fi ODS";

        public const string None = "Learning Standards";
    }
}
