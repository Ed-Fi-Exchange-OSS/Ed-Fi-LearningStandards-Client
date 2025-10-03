// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Extensions;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace EdFi.Admin.LearningStandards.Examples
{
    /// <summary>
    /// Console application demonstrating Ed-Fi data validation
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Ed-Fi Data Validation Example");
            Console.WriteLine("============================");

            // Setup dependency injection
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddEdFiDataValidation();

            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetRequiredService<IEdFiDataValidator>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // Run validation examples
            await RunStudentValidationExample(validator, logger);
            Console.WriteLine();
            await RunLearningStandardValidationExample(validator, logger);
            Console.WriteLine();
            RunDescriptorValidationExample(validator, logger);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static async Task RunStudentValidationExample(IEdFiDataValidator validator, ILogger logger)
        {
            Console.WriteLine("Student Data Validation Example");
            Console.WriteLine("-------------------------------");

            var edFiVersion = new EdFiVersionModel(EdFiWebApiVersion.v7x, EdFiDataStandardVersion.DS5_2, new EdFiWebApiInfo());

            // Valid student
            Console.WriteLine("\n1. Validating VALID student data:");
            var validStudent = JObject.FromObject(new
            {
                studentUniqueId = "STU001",
                firstName = "John",
                lastSurname = "Doe",
                birthDate = "2010-01-15"
            });

            var result = await validator.ValidateDataObjectAsync("students", validStudent, edFiVersion);
            Console.WriteLine($"   Result: {(result.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
            if (!result.IsSuccess)
                Console.WriteLine($"   Errors: {result.ErrorMessage}");

            // Invalid student - missing fields
            Console.WriteLine("\n2. Validating INVALID student data (missing required fields):");
            var invalidStudent = JObject.FromObject(new
            {
                firstName = "Jane"
                // Missing: studentUniqueId, lastSurname, birthDate
            });

            result = await validator.ValidateDataObjectAsync("students", invalidStudent, edFiVersion);
            Console.WriteLine($"   Result: {(result.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
            if (!result.IsSuccess)
                Console.WriteLine($"   Errors: {result.ErrorMessage}");

            // Invalid student - future birth date
            Console.WriteLine("\n3. Validating INVALID student data (future birth date):");
            var futureStudent = JObject.FromObject(new
            {
                studentUniqueId = "STU003",
                firstName = "Bob",
                lastSurname = "Smith",
                birthDate = "2030-01-01"
            });

            result = await validator.ValidateDataObjectAsync("students", futureStudent, edFiVersion);
            Console.WriteLine($"   Result: {(result.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
            if (!result.IsSuccess)
                Console.WriteLine($"   Errors: {result.ErrorMessage}");
        }

        private static async Task RunLearningStandardValidationExample(IEdFiDataValidator validator, ILogger logger)
        {
            Console.WriteLine("Learning Standard Data Validation Example");
            Console.WriteLine("----------------------------------------");

            var edFiVersion = new EdFiVersionModel(EdFiWebApiVersion.v7x, EdFiDataStandardVersion.DS5_2, new EdFiWebApiInfo());

            // Valid learning standard
            Console.WriteLine("\n1. Validating VALID learning standard data:");
            var validLearningStandard = JObject.FromObject(new
            {
                learningStandardId = "LS-MATH-001",
                description = "Students will demonstrate proficiency in addition and subtraction of whole numbers",
                academicSubjects = new[]
                {
                    new { academicSubjectDescriptor = "uri://ed-fi.org/AcademicSubjectDescriptor#Mathematics" }
                },
                gradeLevels = new[]
                {
                    new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Second grade" }
                }
            });

            var result = await validator.ValidateDataObjectAsync("learningStandards", validLearningStandard, edFiVersion);
            Console.WriteLine($"   Result: {(result.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
            if (!result.IsSuccess)
                Console.WriteLine($"   Errors: {result.ErrorMessage}");

            // Invalid learning standard - missing academic subjects
            Console.WriteLine("\n2. Validating INVALID learning standard data (missing academic subjects):");
            var invalidLearningStandard = JObject.FromObject(new
            {
                learningStandardId = "LS-ELA-001",
                description = "Students will demonstrate reading comprehension skills",
                gradeLevels = new[]
                {
                    new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Fourth grade" }
                }
                // Missing: academicSubjects
            });

            result = await validator.ValidateDataObjectAsync("learningStandards", invalidLearningStandard, edFiVersion);
            Console.WriteLine($"   Result: {(result.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
            if (!result.IsSuccess)
                Console.WriteLine($"   Errors: {result.ErrorMessage}");
        }

        private static void RunDescriptorValidationExample(IEdFiDataValidator validator, ILogger logger)
        {
            Console.WriteLine("Descriptor Validation Example");
            Console.WriteLine("----------------------------");

            // Valid descriptors
            Console.WriteLine("\n1. Validating VALID descriptor formats:");
            var validDescriptors = JObject.FromObject(new
            {
                academicSubjectDescriptor = "uri://ed-fi.org/AcademicSubjectDescriptor#Mathematics",
                gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Third grade"
            });

            var result = validator.ValidateDescriptors(validDescriptors);
            Console.WriteLine($"   Result: {(result.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
            if (!result.IsSuccess)
                Console.WriteLine($"   Errors: {result.ErrorMessage}");

            // Invalid descriptors
            Console.WriteLine("\n2. Validating INVALID descriptor formats:");
            var invalidDescriptors = JObject.FromObject(new
            {
                academicSubjectDescriptor = "Mathematics", // Missing URI format
                gradeLevelDescriptor = "http://ed-fi.org/GradeLevelDescriptor#Third grade", // Wrong protocol
                assessmentCategoryDescriptor = "uri://ed-fi.org/AssessmentCategoryDescriptor#Formative" // Valid
            });

            result = validator.ValidateDescriptors(invalidDescriptors);
            Console.WriteLine($"   Result: {(result.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
            if (!result.IsSuccess)
                Console.WriteLine($"   Errors: {result.ErrorMessage}");
        }
    }
}
