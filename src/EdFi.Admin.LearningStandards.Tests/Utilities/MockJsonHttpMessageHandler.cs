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
using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Auth;
using Newtonsoft.Json.Linq;

namespace EdFi.Admin.LearningStandards.Tests
{
    /// <summary>
    ///     An <see cref="HttpMessageHandler" /> that stores a collection of <see cref="JToken" /> objects keyed by http route
    ///     to return when executing a matching request.
    /// </summary>
    public class MockJsonHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<ResponseDescriptor> _responses = new List<ResponseDescriptor>();

        private readonly StringBuilder _internalLogs = new StringBuilder();

        /// <summary>
        ///     The total number of calls made to the <see cref="SendAsync" /> method.
        /// </summary>
        public int CallCount { get; private set; }

        public string Logs => _internalLogs.ToString();

        /// <summary>Send an HTTP request as an asynchronous operation.</summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request">request</paramref> was null.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            _internalLogs.AppendLine($"recieved: [{request.Method}] {request.RequestUri}");

            var candidates = _responses.Where(wh => request.RequestUri.LocalPath.EndsWith(wh.Route, StringComparison.InvariantCultureIgnoreCase) || wh.Route == "*").ToList();
            if (candidates.Count > 0)
            {
                var desc = candidates.FirstOrDefault(fd => fd.Route != "*") ?? candidates.First();

                if (desc.Exception != null)
                {
                    throw desc.Exception;
                }

                var ret = new HttpResponseMessage(desc.HttpStatusCode)
                {
                    Content = desc.Response == null ? null : new JsonHttpContent(desc.Response)
                };

                foreach (var header in desc.Headers)
                {
                    ret.Headers.Add(header.Key, header.Value);
                }

                if (candidates.Count > 1)
                {
                    _responses.Remove(desc);
                }

                if (desc.Delay > TimeSpan.FromSeconds(0))
                {
                    await Task.Delay(desc.Delay, CancellationToken.None).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                _internalLogs.AppendLine($"responded: [{ret.StatusCode}] Body Length: {desc.Response?.ToString().Length}");
                return ret;
            }
            _internalLogs.AppendLine($"responded: [404] Body Length: Not Found");
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        public MockJsonHttpMessageHandler AddRouteResponse(string route, JToken response, string singleHeaderName, string singleHeaderValue)
        {
            return AddRouteResponse(route, HttpStatusCode.OK, response, TimeSpan.FromSeconds(0), new Dictionary<string, string> { { singleHeaderName, singleHeaderValue } });
        }

        /// <summary>
        ///     Adds the specified response parameters to the routed collection for use.
        /// </summary>
        /// <param name="route">The route to match when processing an <see cref="HttpRequestMessage" /></param>
        /// <param name="response">The <see cref="JToken" /> to use as the response body.</param>
        /// <returns>This <see cref="MockJsonHttpMessageHandler" /> for method chaining, if desired.</returns>
        public MockJsonHttpMessageHandler AddRouteResponse(string route, JToken response)
        {
            return AddRouteResponse(route, HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Adds the specified response parameters to the routed collection for use.
        /// </summary>
        /// <param name="route">he route to match when processing an <see cref="HttpRequestMessage" /></param>
        /// <param name="statusCode">The <see cref="HttpStatusCode" /> to use when returning the response.</param>
        /// <returns>This <see cref="MockJsonHttpMessageHandler" /> for method chaining, if desired.</returns>
        public MockJsonHttpMessageHandler AddRouteResponse(string route, HttpStatusCode statusCode)
        {
            return AddRouteResponse(route, statusCode, null, TimeSpan.FromSeconds(0));
        }

        /// <summary>
        ///     Adds the specified response parameters to the routed collection for use.
        /// </summary>
        /// <param name="route">The route to match when processing an <see cref="HttpRequestMessage" /></param>
        /// <param name="statusCode">The <see cref="HttpStatusCode" /> to use when returning the response.</param>
        /// <param name="response">The <see cref="JToken" /> to use as the response body.</param>
        /// <returns>This <see cref="MockJsonHttpMessageHandler" /> for method chaining, if desired.</returns>
        public MockJsonHttpMessageHandler AddRouteResponse(string route, HttpStatusCode statusCode, JToken response)
        {
            return AddRouteResponse(route, statusCode, response, TimeSpan.FromSeconds(0));
        }

        /// <summary>
        ///     Adds the specified response parameters to the routed collection for use.
        /// </summary>
        /// <param name="route">The route to match when processing an <see cref="HttpRequestMessage" /></param>
        /// <param name="statusCode">The <see cref="HttpStatusCode" /> to use when returning the response.</param>
        /// <param name="response">The <see cref="JToken" /> to use as the response body.</param>
        /// <param name="delay">The optional delay to use when returning a reqest (to simulate network lag or failure).</param>
        /// <returns>This <see cref="MockJsonHttpMessageHandler" /> for method chaining, if desired.</returns>
        public MockJsonHttpMessageHandler AddRouteResponse(string route, HttpStatusCode statusCode, JToken response, TimeSpan delay)
        {
            return AddRouteResponse(route, statusCode, response, delay, null);
        }

        public MockJsonHttpMessageHandler AddRouteResponse(string route, HttpStatusCode statusCode, JToken response, TimeSpan delay, Dictionary<string, string> headers)
        {
            _responses.Add(new ResponseDescriptor
            {
                Route = route,
                HttpStatusCode = statusCode,
                Response = response,
                Delay = delay,
                Headers = headers ?? new Dictionary<string, string>()
            });
            return this;
        }

        public MockJsonHttpMessageHandler AddRouteResponse(string route, Exception exception)
        {
            return AddRouteResponse(route, exception, TimeSpan.FromSeconds(0));
        }

        public MockJsonHttpMessageHandler AddRouteResponse(string route, Exception exception, TimeSpan delay)
        {
            _responses.Add(new ResponseDescriptor
            {
                Route = route,
                Exception = exception,
                Delay = delay
            });
            return this;
        }

        private class ResponseDescriptor
        {
            public string Route { get; set; }

            public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(0);

            public JToken Response { get; set; }

            public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;

            public Exception Exception { get; set; }

            public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        }
    }
}
