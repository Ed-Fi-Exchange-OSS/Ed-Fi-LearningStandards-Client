// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Admin.LearningStandards.Core.Auth;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdFi.Admin.LearningStandards.Core.Services
{
    public class EdFiBulkJsonPersister : IEdFiBulkJsonPersister
    {
        private readonly IEdFiOdsApiConfiguration _odsApiConfiguration;
        private readonly IEdFiVersionManager _edFiVersionManager;
        private readonly IAuthTokenManager _odsApiAuthTokenManager;
        private readonly ILogger<EdFiBulkJsonPersister> _logger;
        private readonly HttpClient _httpClient;
        private readonly IEdFiDataValidator _dataValidator;



        public EdFiBulkJsonPersister(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IEdFiVersionManager edFiVersionManager,
            IAuthTokenManager odsApiAuthTokenManager,
            ILogger<EdFiBulkJsonPersister> logger,
            HttpClient httpClient,
            IEdFiDataValidator dataValidator)
        {
            _odsApiConfiguration = odsApiConfiguration;
            _edFiVersionManager = edFiVersionManager;
            _odsApiAuthTokenManager = odsApiAuthTokenManager;
            _logger = logger;
            _httpClient = httpClient;
            _dataValidator = dataValidator;
        }

        public async Task<EdFiVersionModel> GetEdFiVersion()
        {
            return await _edFiVersionManager.GetEdFiVersion(_odsApiConfiguration);
        }

        public async Task<IList<IResponse>> PostEdFiBulkJson(EdFiBulkJsonModel edFiBulkJson, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(edFiBulkJson.Operation)
                && !edFiBulkJson.Operation.Equals("Upsert", StringComparison.InvariantCultureIgnoreCase))
            {
                // Note: Default is upsert.
                throw new NotSupportedException("Only Upserts Supported");
            }

            Check.NotEmpty(edFiBulkJson.Resource, nameof(edFiBulkJson.Resource));
            Check.NotNull(edFiBulkJson.Data, nameof(edFiBulkJson.Data));

            // Pre-validate all data before submission
            _logger.LogDebug("Starting pre-validation for {Resource} with {Count} items",
                edFiBulkJson.Resource, edFiBulkJson.Data.Count);

            var edFiVersion = await _edFiVersionManager.GetEdFiVersion(_odsApiConfiguration);
            var validationResult = await _dataValidator.ValidateBulkJsonModelAsync(edFiBulkJson, edFiVersion);

            if (!validationResult.IsSuccess)
            {
                _logger.LogError("Pre-validation failed for {Resource}: {ValidationErrors}",
                    edFiBulkJson.Resource, validationResult.ErrorMessage);

                // Return validation error as response
                return new List<IResponse> { validationResult };
            }

            _logger.LogDebug("Pre-validation successful for {Resource}", edFiBulkJson.Resource);

            //var odsResourceUrl = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(
            //    _odsApiConfiguration.Url,
            //    edFiBulkJson.Schema,
            //    edFiBulkJson.Resource,
            //    _odsApiConfiguration.Version,
            //    _odsApiConfiguration.SchoolYear);
            var odsResourceUrl = await _edFiVersionManager.ResolveResourceUrl(_odsApiConfiguration, edFiBulkJson.Schema, edFiBulkJson.Resource).ConfigureAwait(false);

            _logger.LogDebug($"Url for Batch Json Model derived as: {odsResourceUrl}");

            var responses = new List<IResponse>();

            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug(
                $"Beginning Post for Batch Json Model. Resource: {edFiBulkJson.Resource} Schema: {edFiBulkJson.Schema}");

            foreach (var resourceData in edFiBulkJson.Data)
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, odsResourceUrl);

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    await _odsApiAuthTokenManager.GetTokenAsync().ConfigureAwait(false));

                // The request content is saved off as a string here instead of being read out of the
                // response object because .NET Full stack doesn't have the fix that was made for .Net Core.
                // See: https://github.com/dotnet/corefx/pull/19082
                string requestContent = resourceData.ToString(Formatting.None);

                httpRequest.Content = new StringContent(
                    requestContent,
                    new UTF8Encoding(),
                    "application/json");

                responses.Add(
                    await ProcessHttpResponseMessage(
                            await _httpClient.SendAsync(httpRequest, cancellationToken)
                                             .ConfigureAwait(false), requestContent)
                        .ConfigureAwait(false));
            }

            _logger.LogDebug(
                $"Successfully loaded {responses.Count(x => x.IsSuccess)} of {responses.Count} {edFiBulkJson.Resource} Resources in Batch Json Model.");

            return responses;
        }

        private async Task<IResponse> ProcessHttpResponseMessage(HttpResponseMessage httpResponseMessage, string requestContent)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
                return new ResponseModel(
                    httpResponseMessage.IsSuccessStatusCode,
                    null,
                    null,
                    httpResponseMessage.StatusCode);

            string responseContent = httpResponseMessage.Content != null
                ? await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false)
                : null;

            // Try to parse Ed-Fi API error response for detailed validation errors
            string detailedErrorMessage = ExtractEdFiValidationErrors(responseContent);

            var errorResponse = new ResponseModel(
                httpResponseMessage.IsSuccessStatusCode,
                detailedErrorMessage ?? httpResponseMessage.ReasonPhrase,
                responseContent,
                httpResponseMessage.StatusCode);

            var logMessageBuilder = new StringBuilder();
            logMessageBuilder.AppendLine("While sending the following content to the ODS API:");
            logMessageBuilder.AppendLine(requestContent);
            logMessageBuilder.AppendLine("The following error occurred:");
            logMessageBuilder.AppendLine($"HttpStatusCode: {errorResponse.StatusCode}");
            logMessageBuilder.AppendLine($"Message: {errorResponse.ErrorMessage}");
            logMessageBuilder.AppendLine($"Response: {errorResponse.Content}");

            _logger.LogError(logMessageBuilder.ToString());

            return errorResponse;
        }

        /// <summary>
        /// Extracts detailed validation error information from Ed-Fi API error responses
        /// </summary>
        private string ExtractEdFiValidationErrors(string responseContent)
        {
            if (string.IsNullOrEmpty(responseContent))
                return null;

            try
            {
                var errorResponse = JObject.Parse(responseContent);

                // Ed-Fi API v4.0 error structure
                var validationErrors = errorResponse["validationErrors"];
                if (validationErrors != null)
                {
                    var errorMessages = new List<string>();

                    foreach (var property in validationErrors.Children<JProperty>())
                    {
                        var fieldName = property.Name.Replace("$.", "");
                        var errors = property.Value.ToObject<string[]>();

                        if (errors != null && errors.Length > 0)
                        {
                            errorMessages.Add($"Field '{fieldName}': {string.Join(", ", errors)}");
                        }
                    }

                    if (errorMessages.Any())
                    {
                        return $"Ed-Fi API Validation Errors: {string.Join("; ", errorMessages)}";
                    }
                }

                // Check for general error details
                var detail = errorResponse["detail"]?.ToString();
                var title = errorResponse["title"]?.ToString();
                var correlationId = errorResponse["correlationId"]?.ToString();

                if (!string.IsNullOrEmpty(detail) || !string.IsNullOrEmpty(title))
                {
                    var message = $"{title}: {detail}";
                    if (!string.IsNullOrEmpty(correlationId))
                    {
                        message += $" (CorrelationId: {correlationId})";
                    }
                    return message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Ed-Fi API error response: {ResponseContent}", responseContent);
            }

            return null;
        }
    }
}
