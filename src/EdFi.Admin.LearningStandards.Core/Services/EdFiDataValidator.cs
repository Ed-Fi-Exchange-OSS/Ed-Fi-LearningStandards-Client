// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EdFi.Admin.LearningStandards.Core.Services
{
    /// <summary>
    /// Provides comprehensive validation for Ed-Fi data objects before API submission
    /// </summary>
    public class EdFiDataValidator : IEdFiDataValidator
    {
        private readonly ILogger<EdFiDataValidator> _logger;

        // Ed-Fi 4.0 Data Standard validation patterns
        private static readonly Regex DatePattern = new Regex(
            @"^\d{4}-\d{2}-\d{2}$",
            RegexOptions.Compiled
        );
        private static readonly Regex DescriptorPattern = new Regex(
            @"^uri://[a-zA-Z0-9\-\.]+/[a-zA-Z]+Descriptor#.+$",
            RegexOptions.Compiled
        );
        private static readonly Regex EmailPattern = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled
        );

        // Required fields mapping for Ed-Fi 4.0 resources
        private static readonly Dictionary<string, string[]> RequiredFieldsMap = new Dictionary<
            string,
            string[]
        >
        {
            ["students"] = new[] { "studentUniqueId", "firstName", "lastSurname", "birthDate" },
            ["learningStandards"] = new[] { "learningStandardId", "description" },
            ["schools"] = new[] { "schoolId", "nameOfInstitution" },
            ["schoolYearTypes"] = new[] { "schoolYear", "currentSchoolYear" },
            ["educationOrganizations"] = new[] { "educationOrganizationId", "nameOfInstitution" },
            ["academicSubjectDescriptors"] = new[]
            {
                "academicSubjectDescriptorId",
                "codeValue",
                "namespace",
            },
            ["gradeLevelDescriptors"] = new[]
            {
                "gradeLevelDescriptorId",
                "codeValue",
                "namespace",
            },
            ["publicationStatusDescriptors"] = new[]
            {
                "publicationStatusDescriptorId",
                "codeValue",
                "namespace",
            },
            ["assessments"] = new[] { "assessmentIdentifier", "namespace", "assessmentTitle" },
            ["courses"] = new[] { "courseCode", "courseTitle", "educationOrganizationReference" },
            ["sections"] = new[] { "sectionIdentifier", "courseOfferingReference" },
            ["staff"] = new[] { "staffUniqueId", "firstName", "lastSurname" },
            ["studentAssessments"] = new[]
            {
                "assessmentReference",
                "studentReference",
                "administrationDate",
            },
        };

        // String length limits for Ed-Fi 4.0 fields
        private static readonly Dictionary<string, int> StringLengthLimits = new Dictionary<
            string,
            int
        >
        {
            ["firstName"] = 75,
            ["lastSurname"] = 75,
            ["middleName"] = 75,
            ["description"] = 1024,
            ["shortDescription"] = 255,
            ["nameOfInstitution"] = 75,
            ["courseTitle"] = 60,
            ["sectionIdentifier"] = 255,
            ["assessmentTitle"] = 255,
            ["codeValue"] = 50,
            ["namespace"] = 255,
        };

        public EdFiDataValidator(ILogger<EdFiDataValidator> logger)
        {
            _logger = logger;
        }

        public Task<IResponse> ValidateDataObjectAsync(
            string resourceType,
            JObject data,
            EdFiVersionModel edFiVersion
        )
        {
            try
            {
                var validationResults = new List<IResponse>
                {
                    ValidateRequiredFields(resourceType, data),
                    ValidateDataTypes(resourceType, data),
                    ValidateDescriptors(data),
                    ValidateBusinessRules(resourceType, data),
                };

                var aggregateResult = ResponseModel.Aggregate(validationResults);

                if (!aggregateResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Validation failed for {ResourceType}: {Errors}",
                        resourceType,
                        aggregateResult.ErrorMessage
                    );
                }

                return Task.FromResult(aggregateResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating {ResourceType} data object", resourceType);
                return Task.FromResult<IResponse>(
                    ResponseModel.Error(
                        $"Validation error: {ex.Message}",
                        HttpStatusCode.BadRequest
                    )
                );
            }
        }

        public async Task<IResponse> ValidateDataObjectsAsync(
            string resourceType,
            IEnumerable<JObject> dataObjects,
            EdFiVersionModel edFiVersion
        )
        {
            var validationTasks = dataObjects.Select(data =>
                ValidateDataObjectAsync(resourceType, data, edFiVersion)
            );
            var results = await Task.WhenAll(validationTasks);

            return ResponseModel.Aggregate(results);
        }

        public async Task<IResponse> ValidateBulkJsonModelAsync(
            EdFiBulkJsonModel bulkJsonModel,
            EdFiVersionModel edFiVersion
        )
        {
            try
            {
                var validationErrors = new List<string>();

                // Validate bulk model structure
                if (string.IsNullOrEmpty(bulkJsonModel.Resource))
                {
                    validationErrors.Add("Resource type is required");
                }

                if (bulkJsonModel.Data == null || !bulkJsonModel.Data.Any())
                {
                    validationErrors.Add("Data collection cannot be null or empty");
                }

                if (
                    !string.IsNullOrEmpty(bulkJsonModel.Operation)
                    && !bulkJsonModel.Operation.Equals(
                        "Upsert",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                {
                    validationErrors.Add("Only 'Upsert' operations are supported");
                }

                if (validationErrors.Any())
                {
                    return ResponseModel.Error(
                        string.Join("; ", validationErrors),
                        HttpStatusCode.BadRequest
                    );
                }

                // Validate individual data objects
                var dataValidationResult = await ValidateDataObjectsAsync(
                    bulkJsonModel.Resource,
                    bulkJsonModel.Data,
                    edFiVersion
                );

                if (!dataValidationResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Bulk validation failed for {Resource} with {Count} items: {Errors}",
                        bulkJsonModel.Resource,
                        bulkJsonModel.Data.Count,
                        dataValidationResult.ErrorMessage
                    );
                }

                return dataValidationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error validating bulk JSON model for {Resource}",
                    bulkJsonModel.Resource
                );
                return ResponseModel.Error(
                    $"Bulk validation error: {ex.Message}",
                    HttpStatusCode.BadRequest
                );
            }
        }

        public IResponse ValidateRequiredFields(string resourceType, JObject data)
        {
            var errors = new List<string>();

            if (!RequiredFieldsMap.TryGetValue(resourceType, out var requiredFields))
            {
                _logger.LogWarning(
                    "No required field mapping found for resource type: {ResourceType}",
                    resourceType
                );
                return ResponseModel.Success($"No specific validation rules for {resourceType}");
            }

            foreach (var field in requiredFields)
            {
                var token = data.SelectToken(field);
                if (token == null || string.IsNullOrWhiteSpace(token.ToString()))
                {
                    errors.Add($"Required field '{field}' is missing or empty");
                }
            }

            return errors.Any()
                ? ResponseModel.Error(
                    $"Required field validation failed: {string.Join(", ", errors)}",
                    HttpStatusCode.BadRequest
                )
                : ResponseModel.Success("Required fields validation passed");
        }

        public IResponse ValidateDataTypes(string resourceType, JObject data)
        {
            var errors = new List<string>();

            foreach (var property in data.Properties())
            {
                var value = property.Value;
                var fieldName = property.Name;

                // Validate date fields
                if (fieldName.ToLower().Contains("date") && value.Type == JTokenType.String)
                {
                    var dateString = value.ToString();
                    if (!string.IsNullOrEmpty(dateString) && !DatePattern.IsMatch(dateString))
                    {
                        errors.Add(
                            $"Field '{fieldName}' must be in YYYY-MM-DD format, got: '{dateString}'"
                        );
                    }
                    else if (
                        !string.IsNullOrEmpty(dateString)
                        && !DateTime.TryParseExact(
                            dateString,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out _
                        )
                    )
                    {
                        errors.Add($"Field '{fieldName}' is not a valid date: '{dateString}'");
                    }
                }

                // Validate email fields
                if (fieldName.ToLower().Contains("email") && value.Type == JTokenType.String)
                {
                    var emailString = value.ToString();
                    if (!string.IsNullOrEmpty(emailString) && !EmailPattern.IsMatch(emailString))
                    {
                        errors.Add(
                            $"Field '{fieldName}' is not a valid email address: '{emailString}'"
                        );
                    }
                }

                // Validate string length limits
                if (
                    value.Type == JTokenType.String
                    && StringLengthLimits.TryGetValue(fieldName, out var maxLength)
                )
                {
                    var stringValue = value.ToString();
                    if (stringValue.Length > maxLength)
                    {
                        errors.Add(
                            $"Field '{fieldName}' exceeds maximum length of {maxLength} characters (current: {stringValue.Length})"
                        );
                    }
                }

                // Validate numeric fields
                if (
                    fieldName.ToLower().Contains("id")
                    && fieldName != "learningStandardId"
                    && fieldName != "studentUniqueId"
                )
                {
                    if (value.Type == JTokenType.String)
                    {
                        var stringValue = value.ToString();
                        if (!string.IsNullOrEmpty(stringValue) && !int.TryParse(stringValue, out _))
                        {
                            errors.Add(
                                $"Field '{fieldName}' should be numeric, got: '{stringValue}'"
                            );
                        }
                    }
                    else if (value.Type != JTokenType.Integer && value.Type != JTokenType.Null)
                    {
                        errors.Add($"Field '{fieldName}' should be numeric");
                    }
                }
            }

            return errors.Any()
                ? ResponseModel.Error(
                    $"Data type validation failed: {string.Join(", ", errors)}",
                    HttpStatusCode.BadRequest
                )
                : ResponseModel.Success("Data type validation passed");
        }

        public IResponse ValidateDescriptors(JObject data)
        {
            var errors = new List<string>();

            foreach (var property in data.Properties())
            {
                ValidateDescriptorProperty(property, errors, property.Name);
            }

            return errors.Any()
                ? ResponseModel.Error(
                    $"Descriptor validation failed: {string.Join(", ", errors)}",
                    HttpStatusCode.BadRequest
                )
                : ResponseModel.Success("Descriptor validation passed");
        }

        private void ValidateDescriptorProperty(
            JProperty property,
            List<string> errors,
            string path
        )
        {
            var fieldName = property.Name;
            var value = property.Value;

            // Check if this is a descriptor field
            if (
                fieldName.EndsWith("Descriptor", StringComparison.OrdinalIgnoreCase)
                && value.Type == JTokenType.String
            )
            {
                var descriptorValue = value.ToString();
                if (
                    !string.IsNullOrEmpty(descriptorValue)
                    && !DescriptorPattern.IsMatch(descriptorValue)
                )
                {
                    errors.Add(
                        $"Descriptor '{path}' has invalid URI format: '{descriptorValue}'. Expected format: 'uri://domain/DescriptorType#Value'"
                    );
                }
            }

            // Recursively check nested objects and arrays
            if (value.Type == JTokenType.Object)
            {
                foreach (var nestedProperty in ((JObject)value).Properties())
                {
                    ValidateDescriptorProperty(
                        nestedProperty,
                        errors,
                        $"{path}.{nestedProperty.Name}"
                    );
                }
            }
            else if (value.Type == JTokenType.Array)
            {
                for (int i = 0; i < value.Count(); i++)
                {
                    var arrayItem = value[i];
                    if (arrayItem.Type == JTokenType.Object)
                    {
                        foreach (var nestedProperty in ((JObject)arrayItem).Properties())
                        {
                            ValidateDescriptorProperty(
                                nestedProperty,
                                errors,
                                $"{path}[{i}].{nestedProperty.Name}"
                            );
                        }
                    }
                }
            }
        }

        public IResponse ValidateBusinessRules(string resourceType, JObject data)
        {
            var errors = new List<string>();

            switch (resourceType.ToLower())
            {
                case "students":
                    ValidateStudentBusinessRules(data, errors);
                    break;
                case "learningstandards":
                    ValidateLearningStandardBusinessRules(data, errors);
                    break;
                case "studentassessments":
                    ValidateStudentAssessmentBusinessRules(data, errors);
                    break;
                case "courses":
                    ValidateCourseBusinessRules(data, errors);
                    break;
                default:
                    // Generic validation for unknown resource types
                    break;
            }

            return errors.Any()
                ? ResponseModel.Error(
                    $"Business rule validation failed: {string.Join(", ", errors)}",
                    HttpStatusCode.BadRequest
                )
                : ResponseModel.Success("Business rule validation passed");
        }

        private void ValidateStudentBusinessRules(JObject data, List<string> errors)
        {
            // Validate birth date is not in the future
            var birthDateToken = data.SelectToken("birthDate");
            if (
                birthDateToken != null
                && DateTime.TryParseExact(
                    birthDateToken.ToString(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var birthDate
                )
            )
            {
                if (birthDate > DateTime.Today)
                {
                    errors.Add("Birth date cannot be in the future");
                }

                // Validate reasonable age range (not more than 150 years old)
                if (DateTime.Today.Year - birthDate.Year > 150)
                {
                    errors.Add("Birth date indicates age greater than 150 years");
                }
            }

            // Validate grade levels are reasonable
            var gradeLevels = data.SelectToken(
                "studentEducationOrganizationAssociations[*].gradeLevels[*].gradeLevelDescriptor"
            );
            if (gradeLevels != null)
            {
                foreach (var gradeLevel in gradeLevels)
                {
                    var gradeLevelStr = gradeLevel.ToString();
                    if (!string.IsNullOrEmpty(gradeLevelStr) && !IsValidGradeLevel(gradeLevelStr))
                    {
                        errors.Add($"Invalid grade level descriptor: {gradeLevelStr}");
                    }
                }
            }
        }

        private void ValidateLearningStandardBusinessRules(JObject data, List<string> errors)
        {
            // Validate description length is reasonable
            var description = data.SelectToken("description")?.ToString();
            if (!string.IsNullOrEmpty(description))
            {
                if (description.Length < 10)
                {
                    errors.Add(
                        "Learning standard description should be at least 10 characters long"
                    );
                }
                if (description.Length > 1024)
                {
                    errors.Add(
                        "Learning standard description exceeds maximum length of 1024 characters"
                    );
                }
            }

            // Validate course title length if present
            var courseTitle = data.SelectToken("courseTitle")?.ToString();
            if (!string.IsNullOrEmpty(courseTitle) && courseTitle.Length > 60)
            {
                errors.Add("Course title exceeds maximum length of 60 characters");
            }

            // Validate academic subjects are present
            var academicSubjects = data.SelectToken("academicSubjects");
            if (academicSubjects == null || !academicSubjects.Any())
            {
                errors.Add("Learning standard must have at least one academic subject");
            }

            // Validate grade levels are present
            var gradeLevels = data.SelectToken("gradeLevels");
            if (gradeLevels == null || !gradeLevels.Any())
            {
                errors.Add("Learning standard must have at least one grade level");
            }
        }

        private void ValidateStudentAssessmentBusinessRules(JObject data, List<string> errors)
        {
            // Validate administration date is not in the future
            var administrationDate = data.SelectToken("administrationDate");
            if (
                administrationDate != null
                && DateTime.TryParseExact(
                    administrationDate.ToString(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var adminDate
                )
            )
            {
                if (adminDate > DateTime.Today)
                {
                    errors.Add("Assessment administration date cannot be in the future");
                }
            }

            // Validate score results are within reasonable ranges
            var scoreResults = data.SelectTokens("scoreResults[*]");
            foreach (var scoreResult in scoreResults)
            {
                var result = scoreResult.SelectToken("result")?.ToString();
                if (!string.IsNullOrEmpty(result) && decimal.TryParse(result, out var score))
                {
                    if (score < 0)
                    {
                        errors.Add("Assessment score cannot be negative");
                    }
                    if (score > 9999) // Reasonable upper limit
                    {
                        errors.Add("Assessment score seems unreasonably high (>9999)");
                    }
                }
            }
        }

        private void ValidateCourseBusinessRules(JObject data, List<string> errors)
        {
            // Validate course code format
            var courseCode = data.SelectToken("courseCode")?.ToString();
            if (!string.IsNullOrEmpty(courseCode))
            {
                if (courseCode.Length > 60)
                {
                    errors.Add("Course code exceeds maximum length of 60 characters");
                }
                if (string.IsNullOrWhiteSpace(courseCode))
                {
                    errors.Add("Course code cannot be empty or whitespace only");
                }
            }

            // Validate course title
            var courseTitle = data.SelectToken("courseTitle")?.ToString();
            if (!string.IsNullOrEmpty(courseTitle) && courseTitle.Length > 60)
            {
                errors.Add("Course title exceeds maximum length of 60 characters");
            }
        }

        private bool IsValidGradeLevel(string gradeLevelDescriptor)
        {
            // Ed-Fi 4.0 standard grade level descriptors
            var validGradeLevels = new[]
            {
                "Kindergarten",
                "First grade",
                "Second grade",
                "Third grade",
                "Fourth grade",
                "Fifth grade",
                "Sixth grade",
                "Seventh grade",
                "Eighth grade",
                "Ninth grade",
                "Tenth grade",
                "Eleventh grade",
                "Twelfth grade",
                "Prekindergarten",
                "Adult Education",
                "Postsecondary",
                "Ungraded",
            };

            return validGradeLevels.Any(level => gradeLevelDescriptor.Contains(level));
        }
    }
}
