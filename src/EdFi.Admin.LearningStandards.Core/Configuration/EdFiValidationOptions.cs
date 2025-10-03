// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;

namespace EdFi.Admin.LearningStandards.Core.Configuration
{
    /// <summary>
    /// Configuration options for Ed-Fi data validation
    /// </summary>
    public class EdFiValidationOptions
    {
        /// <summary>
        /// Whether to enable pre-validation before API submission
        /// </summary>
        public bool EnablePreValidation { get; set; } = true;

        /// <summary>
        /// Whether to fail fast on first validation error or collect all errors
        /// </summary>
        public bool FailFast { get; set; } = false;

        /// <summary>
        /// Maximum number of validation errors to collect per object
        /// </summary>
        public int MaxErrorsPerObject { get; set; } = 100;

        /// <summary>
        /// Whether to validate descriptors against Ed-Fi standard values
        /// </summary>
        public bool ValidateDescriptors { get; set; } = true;

        /// <summary>
        /// Whether to validate business rules
        /// </summary>
        public bool ValidateBusinessRules { get; set; } = true;

        /// <summary>
        /// Whether to validate string length limits
        /// </summary>
        public bool ValidateStringLengths { get; set; } = true;

        /// <summary>
        /// Whether to validate date formats and ranges
        /// </summary>
        public bool ValidateDates { get; set; } = true;

        /// <summary>
        /// Custom required fields mapping for resources not covered by default rules
        /// </summary>
        public Dictionary<string, string[]> CustomRequiredFields { get; set; } = new Dictionary<string, string[]>();

        /// <summary>
        /// Custom string length limits for fields not covered by default rules
        /// </summary>
        public Dictionary<string, int> CustomStringLengthLimits { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Whether to include detailed validation context in error messages
        /// </summary>
        public bool IncludeDetailedContext { get; set; } = true;

        /// <summary>
        /// Custom validation rules that can be applied to specific resource types
        /// </summary>
        public Dictionary<string, List<string>> CustomValidationRules { get; set; } = new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// Ed-Fi API error handling configuration
    /// </summary>
    public class EdFiErrorHandlingOptions
    {
        /// <summary>
        /// Whether to retry on validation errors
        /// </summary>
        public bool RetryOnValidationErrors { get; set; } = false;

        /// <summary>
        /// Maximum number of retries for transient errors
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retries in milliseconds
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Whether to log detailed error information
        /// </summary>
        public bool LogDetailedErrors { get; set; } = true;

        /// <summary>
        /// Whether to extract and parse Ed-Fi API error responses
        /// </summary>
        public bool ParseApiErrorResponses { get; set; } = true;
    }
}
