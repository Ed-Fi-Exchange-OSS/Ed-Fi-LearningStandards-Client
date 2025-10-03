// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Extensions;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdFi.Admin.LearningStandards.Examples
{
    /// <summary>
    /// Example implementation showing how to use the Ed-Fi data validation system
    /// </summary>
    public class EdFiValidationExample
    {
        private readonly IEdFiDataValidator _validator;
        private readonly ILogger<EdFiValidationExample> _logger;

        public EdFiValidationExample(IEdFiDataValidator validator, ILogger<EdFiValidationExample> logger)
        {
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// Example of validating student data before submission to Ed-Fi API
        /// </summary>
        public async Task<bool> ValidateAndSubmitStudentData()
        {
            _logger.LogInformation("Starting student data validation example");

            // Create sample student data - some valid, some invalid
            var studentData = new List<JObject>
            {
                // Valid student
                JObject.FromObject(new
                {
                    studentUniqueId = "STU001",
                    firstName = "John",
                    middleName = "Michael",
                    lastSurname = "Doe",
                    birthDate = "2010-01-15",
                    studentEducationOrganizationAssociations = new[]
                    {
                        new
                        {
                            educationOrganizationReference = new { educationOrganizationId = 123456 },
                            gradeLevels = new[]
                            {
                                new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Third grade" }
                            }
                        }
                    }
                }),

                // Invalid student - missing required fields
                JObject.FromObject(new
                {
                    firstName = "Jane",
                    // Missing: studentUniqueId, lastSurname, birthDate
                }),

                // Invalid student - future birth date
                JObject.FromObject(new
                {
                    studentUniqueId = "STU003",
                    firstName = "Bob",
                    lastSurname = "Smith",
                    birthDate = "2030-01-01" // Future date - invalid
                }),

                // Invalid student - field too long
                JObject.FromObject(new
                {
                    studentUniqueId = "STU004",
                    firstName = new string('A', 100), // Exceeds 75 character limit
                    lastSurname = "Johnson",
                    birthDate = "2011-05-20"
                })
            };

            // Create Ed-Fi version for validation context
            var edFiVersion = new EdFiVersionModel(
                EdFiWebApiVersion.v7x,
                EdFiDataStandardVersion.DS5_2,
                new EdFiWebApiInfo()
            );

            // Validate each student individually
            _logger.LogInformation("Validating {Count} student records", studentData.Count);

            var validationResults = new List<IResponse>();
            for (int i = 0; i < studentData.Count; i++)
            {
                var student = studentData[i];
                var result = await _validator.ValidateDataObjectAsync("students", student, edFiVersion);
                validationResults.Add(result);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Student {Index} validation passed", i + 1);
                }
                else
                {
                    _logger.LogError("Student {Index} validation failed: {Errors}", i + 1, result.ErrorMessage);
                }
            }

            // Aggregate validation results
            var aggregateResult = ResponseModel.Aggregate(validationResults);

            if (aggregateResult.IsSuccess)
            {
                _logger.LogInformation("All student data validation passed. Ready for API submission.");
                return true;
            }
            else
            {
                _logger.LogError("Student data validation failed. Cannot submit to Ed-Fi API: {Errors}",
                    aggregateResult.ErrorMessage);
                return false;
            }
        }

        /// <summary>
        /// Example of validating learning standards data
        /// </summary>
        public async Task<bool> ValidateLearningStandardsData()
        {
            _logger.LogInformation("Starting learning standards validation example");

            var learningStandardsData = new List<JObject>
            {
                // Valid learning standard
                JObject.FromObject(new
                {
                    learningStandardId = "LS-MATH-001",
                    description = "Students will demonstrate proficiency in addition and subtraction of whole numbers",
                    academicSubjects = new[]
                    {
                        new { academicSubjectDescriptor = "uri://ed-fi.org/AcademicSubjectDescriptor#Mathematics" }
                    },
                    gradeLevels = new[]
                    {
                        new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Second grade" },
                        new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Third grade" }
                    },
                    contentStandard = new
                    {
                        title = "Common Core State Standards",
                        publicationStatusDescriptor = "uri://ed-fi.org/PublicationStatusDescriptor#Active",
                        publicationYear = 2010
                    }
                }),

                // Invalid learning standard - missing academic subjects
                JObject.FromObject(new
                {
                    learningStandardId = "LS-ELA-001",
                    description = "Students will demonstrate reading comprehension skills",
                    gradeLevels = new[]
                    {
                        new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Fourth grade" }
                    }
                    // Missing: academicSubjects
                }),

                // Invalid learning standard - description too short
                JObject.FromObject(new
                {
                    learningStandardId = "LS-SCI-001",
                    description = "Science", // Too short
                    academicSubjects = new[]
                    {
                        new { academicSubjectDescriptor = "uri://ed-fi.org/AcademicSubjectDescriptor#Science" }
                    },
                    gradeLevels = new[]
                    {
                        new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Fifth grade" }
                    }
                })
            };

            // Create bulk JSON model for validation
            var bulkJsonModel = new EdFiBulkJsonModel
            {
                Resource = "learningStandards",
                Schema = "ed-fi",
                Operation = "Upsert",
                Data = learningStandardsData
            };

            var edFiVersion = new EdFiVersionModel(
                EdFiWebApiVersion.v7x,
                EdFiDataStandardVersion.DS5_2,
                new EdFiWebApiInfo()
            );

            // Validate the entire bulk model
            var result = await _validator.ValidateBulkJsonModelAsync(bulkJsonModel, edFiVersion);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Learning standards validation passed. Data is ready for submission.");
                return true;
            }
            else
            {
                _logger.LogError("Learning standards validation failed: {Errors}", result.ErrorMessage);
                return false;
            }
        }

        /// <summary>
        /// Example of descriptor validation
        /// </summary>
        public void ValidateDescriptorFormats()
        {
            _logger.LogInformation("Starting descriptor validation example");

            var dataWithDescriptors = JObject.FromObject(new
            {
                // Valid descriptors
                academicSubjectDescriptor = "uri://ed-fi.org/AcademicSubjectDescriptor#Mathematics",
                gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Third grade",

                // Invalid descriptors
                invalidDescriptor1 = "Mathematics", // Missing URI format
                invalidDescriptor2 = "http://ed-fi.org/GradeLevelDescriptor#Third grade", // Wrong protocol

                // Nested object with descriptors
                assessment = new
                {
                    assessmentCategoryDescriptor = "uri://ed-fi.org/AssessmentCategoryDescriptor#Formative",
                    invalidNestedDescriptor = "Formative" // Invalid format
                },

                // Array with descriptors
                scores = new[]
                {
                    new { scoreTypeDescriptor = "uri://ed-fi.org/ScoreTypeDescriptor#Raw Score" },
                    new { scoreTypeDescriptor = "Raw Score" } // Invalid format
                }
            });

            var result = _validator.ValidateDescriptors(dataWithDescriptors);

            if (result.IsSuccess)
            {
                _logger.LogInformation("All descriptors are valid");
            }
            else
            {
                _logger.LogError("Descriptor validation failed: {Errors}", result.ErrorMessage);
            }
        }

        /// <summary>
        /// Example of custom validation configuration
        /// </summary>
        public static void ConfigureCustomValidation(IServiceCollection services)
        {
            // Add Ed-Fi validation services
            services.AddEdFiDataValidation();

            // Configure custom validation options
            services.Configure<EdFiValidationOptions>(options =>
            {
                // Enable all validation features
                options.EnablePreValidation = true;
                options.ValidateDescriptors = true;
                options.ValidateBusinessRules = true;
                options.ValidateStringLengths = true;
                options.ValidateDates = true;

                // Configure error handling
                options.FailFast = false; // Collect all errors
                options.MaxErrorsPerObject = 50;
                options.IncludeDetailedContext = true;

                // Add custom required fields for a custom resource
                options.CustomRequiredFields.Add("customAssessments", new[]
                {
                    "assessmentIdentifier",
                    "assessmentTitle",
                    "customField1"
                });

                // Add custom string length limits
                options.CustomStringLengthLimits.Add("customDescription", 2000);
                options.CustomStringLengthLimits.Add("customTitle", 100);
            });

            // Configure error handling options
            services.Configure<EdFiErrorHandlingOptions>(options =>
            {
                options.RetryOnValidationErrors = false;
                options.MaxRetries = 3;
                options.RetryDelayMs = 1000;
                options.LogDetailedErrors = true;
                options.ParseApiErrorResponses = true;
            });
        }

        /// <summary>
        /// Example of handling validation errors in a data processing pipeline
        /// </summary>
        public async Task<ProcessingResult> ProcessStudentDataPipeline(IEnumerable<JObject> studentData)
        {
            var result = new ProcessingResult();
            var edFiVersion = new EdFiVersionModel(EdFiWebApiVersion.v7x, EdFiDataStandardVersion.DS5_2, new EdFiWebApiInfo());

            foreach (var student in studentData)
            {
                try
                {
                    // Validate the student data
                    var validationResult = await _validator.ValidateDataObjectAsync("students", student, edFiVersion);

                    if (validationResult.IsSuccess)
                    {
                        // Process valid data
                        result.SuccessCount++;
                        _logger.LogDebug("Student {StudentId} validation passed",
                            student["studentUniqueId"]?.ToString());
                    }
                    else
                    {
                        // Handle validation errors
                        result.ErrorCount++;
                        result.Errors.Add(new ProcessingError
                        {
                            StudentId = student["studentUniqueId"]?.ToString(),
                            ErrorMessage = validationResult.ErrorMessage,
                            ErrorType = "Validation"
                        });

                        _logger.LogWarning("Student {StudentId} validation failed: {Error}",
                            student["studentUniqueId"]?.ToString(),
                            validationResult.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ProcessingError
                    {
                        StudentId = student["studentUniqueId"]?.ToString(),
                        ErrorMessage = ex.Message,
                        ErrorType = "Processing"
                    });

                    _logger.LogError(ex, "Error processing student {StudentId}",
                        student["studentUniqueId"]?.ToString());
                }
            }

            _logger.LogInformation("Processing completed. Success: {Success}, Errors: {Errors}",
                result.SuccessCount, result.ErrorCount);

            return result;
        }
    }

    /// <summary>
    /// Result of data processing pipeline
    /// </summary>
    public class ProcessingResult
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ProcessingError> Errors { get; set; } = new List<ProcessingError>();

        public bool IsSuccess => ErrorCount == 0;
        public int TotalProcessed => SuccessCount + ErrorCount;
    }

    /// <summary>
    /// Represents a processing error
    /// </summary>
    public class ProcessingError
    {
        public string StudentId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }
    }
}
