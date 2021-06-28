// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using EdFi.Admin.LearningStandards.Core;

namespace System
{
    public static class LearningStandardsExceptionExtensions
    {
        private static readonly Dictionary<Type, HttpStatusCode> _exceptionStatusMap = new Dictionary<Type, HttpStatusCode>
        {
            { typeof(ArgumentException), HttpStatusCode.BadRequest },
            { typeof(ArgumentNullException), HttpStatusCode.BadRequest },
            { typeof(ArgumentOutOfRangeException), HttpStatusCode.BadRequest },
            { typeof(IndexOutOfRangeException), HttpStatusCode.BadRequest },
            { typeof(FormatException), HttpStatusCode.BadRequest },
            { typeof(UriFormatException), HttpStatusCode.BadRequest },
            { typeof(NotImplementedException), HttpStatusCode.NotImplemented },
            { typeof(Exception), HttpStatusCode.InternalServerError },
            { typeof(HttpRequestException), HttpStatusCode.BadRequest }
        };

        public static Exception RollUp(this Exception exception)
        {
            var sb = new StringBuilder();
            var inners = exception.GetInnerExceptions().ToList();
            for (int i = 0; i < inners.Count; i++)
            {
                sb.AppendLine($"Exception {i}: {inners[i]}");
                sb.AppendLine();
            }

            return new Exception(sb.ToString());
        }

        public static IEnumerable<Exception> GetInnerExceptions(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var innerException = exception;
            do
            {
                yield return innerException;
                innerException = innerException.InnerException;
            }
            while (innerException != null);
        }

        public static IResponse ToLearningStandardsResponse(this Exception exception)
        {
            if (exception is LearningStandardsHttpRequestException httpException)
            {
                return new ResponseModel(false, exception.Message, httpException.ResponseContent, httpException.HttpStatusCode);
            }

            if (exception is AggregateException aggregateException)
            {
                return ResponseModel.Aggregate(aggregateException.InnerExceptions.Select(sl => sl.ToLearningStandardsResponse()));
            }


            var statusCode = HttpStatusCode.InternalServerError;
            if (_exceptionStatusMap.ContainsKey(exception.GetType()))
            {
                statusCode = _exceptionStatusMap[exception.GetType()];
            }

            var sb = new StringBuilder();
            string innerRemoval = ", see inner exception";
            sb.Append(exception.Message.Replace(innerRemoval, string.Empty));

            var inner = exception.InnerException;
            if (inner != null)
            {
                sb.Append(" (");
                while (inner != null)
                {
                    sb.Append(inner.Message.Replace(innerRemoval, string.Empty));
                    inner = inner.InnerException;
                }

                sb.Append(") ");
            }

            sb.Replace("\r\n", " ");

            return new ResponseModel(false, sb.ToString(), exception.ToString(), statusCode);
        }
    }
}
