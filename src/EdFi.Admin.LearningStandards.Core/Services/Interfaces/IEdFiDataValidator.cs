// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Admin.LearningStandards.Core.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdFi.Admin.LearningStandards.Core.Services.Interfaces
{
    /// <summary>
    /// Provides validation services for Ed-Fi data before submission to the API
    /// </summary>
    public interface IEdFiDataValidator
    {
        /// <summary>
        /// Validates a single Ed-Fi data object against Ed-Fi 4.0 standards
        /// </summary>
        /// <param name="resourceType">The type of Ed-Fi resource (e.g., "students", "learningStandards")</param>
        /// <param name="data">The JSON data object to validate</param>
        /// <param name="edFiVersion">The Ed-Fi version model for validation context</param>
        /// <returns>Validation result with details about any errors found</returns>
        Task<IResponse> ValidateDataObjectAsync(string resourceType, JObject data, EdFiVersionModel edFiVersion);

        /// <summary>
        /// Validates a collection of Ed-Fi data objects
        /// </summary>
        /// <param name="resourceType">The type of Ed-Fi resource</param>
        /// <param name="dataObjects">Collection of JSON data objects to validate</param>
        /// <param name="edFiVersion">The Ed-Fi version model for validation context</param>
        /// <returns>Validation result aggregating all validation errors</returns>
        Task<IResponse> ValidateDataObjectsAsync(string resourceType, IEnumerable<JObject> dataObjects, EdFiVersionModel edFiVersion);

        /// <summary>
        /// Validates an entire EdFiBulkJsonModel before submission
        /// </summary>
        /// <param name="bulkJsonModel">The bulk JSON model to validate</param>
        /// <param name="edFiVersion">The Ed-Fi version model for validation context</param>
        /// <returns>Validation result with comprehensive error reporting</returns>
        Task<IResponse> ValidateBulkJsonModelAsync(EdFiBulkJsonModel bulkJsonModel, EdFiVersionModel edFiVersion);

        /// <summary>
        /// Validates required fields are present and non-empty
        /// </summary>
        /// <param name="resourceType">The type of Ed-Fi resource</param>
        /// <param name="data">The JSON data object to validate</param>
        /// <returns>Validation result for required fields</returns>
        IResponse ValidateRequiredFields(string resourceType, JObject data);

        /// <summary>
        /// Validates data types match Ed-Fi specifications
        /// </summary>
        /// <param name="resourceType">The type of Ed-Fi resource</param>
        /// <param name="data">The JSON data object to validate</param>
        /// <returns>Validation result for data types</returns>
        IResponse ValidateDataTypes(string resourceType, JObject data);

        /// <summary>
        /// Validates descriptor URIs are properly formatted
        /// </summary>
        /// <param name="data">The JSON data object to validate</param>
        /// <returns>Validation result for descriptors</returns>
        IResponse ValidateDescriptors(JObject data);

        /// <summary>
        /// Validates business rules specific to Ed-Fi entities
        /// </summary>
        /// <param name="resourceType">The type of Ed-Fi resource</param>
        /// <param name="data">The JSON data object to validate</param>
        /// <returns>Validation result for business rules</returns>
        IResponse ValidateBusinessRules(string resourceType, JObject data);
    }
}