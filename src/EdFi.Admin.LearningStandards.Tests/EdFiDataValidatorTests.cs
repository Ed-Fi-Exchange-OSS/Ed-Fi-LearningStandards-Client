// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services;
using EdFi.Admin.LearningStandards.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace EdFi.Admin.LearningStandards.Tests
{
    [TestFixture]
    public class EdFiDataValidatorTests
    {
        private EdFiDataValidator _validator;
        private ILogger<EdFiDataValidator> _logger;
        private EdFiVersionModel _edFiVersion;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _logger = new NUnitConsoleLogger<EdFiDataValidator>();
            _validator = new EdFiDataValidator(_logger);
            _edFiVersion = new EdFiVersionModel(
                EdFiWebApiVersion.v7x,
                EdFiDataStandardVersion.DS5_2,
                new EdFiWebApiInfo()
            );
        }

        [Test]
        public async Task ValidateDataObject_ValidStudent_ShouldPass()
        {
            // Arrange
            var validStudent = JObject.FromObject(new
            {
                studentUniqueId = "12345",
                firstName = "John",
                lastSurname = "Doe",
                birthDate = "2010-01-15"
            });

            // Act
            var result = await _validator.ValidateDataObjectAsync("students", validStudent, _edFiVersion);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task ValidateDataObject_StudentMissingRequiredFields_ShouldFail()
        {
            // Arrange
            var invalidStudent = JObject.FromObject(new
            {
                firstName = "John"
                // Missing studentUniqueId, lastSurname, birthDate
            });

            // Act
            var result = await _validator.ValidateDataObjectAsync("students", invalidStudent, _edFiVersion);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("studentUniqueId"));
            Assert.IsTrue(result.ErrorMessage.Contains("lastSurname"));
            Assert.IsTrue(result.ErrorMessage.Contains("birthDate"));
        }

        [Test]
        public async Task ValidateDataObject_InvalidDateFormat_ShouldFail()
        {
            // Arrange
            var studentWithInvalidDate = JObject.FromObject(new
            {
                studentUniqueId = "12345",
                firstName = "John",
                lastSurname = "Doe",
                birthDate = "01/15/2010" // Invalid format
            });

            // Act
            var result = await _validator.ValidateDataObjectAsync("students", studentWithInvalidDate, _edFiVersion);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("YYYY-MM-DD"));
        }

        [Test]
        public async Task ValidateDataObject_FutureBirthDate_ShouldFail()
        {
            // Arrange
            var studentWithFutureBirthDate = JObject.FromObject(new
            {
                studentUniqueId = "12345",
                firstName = "John",
                lastSurname = "Doe",
                birthDate = "2030-01-15" // Future date
            });

            // Act
            var result = await _validator.ValidateDataObjectAsync("students", studentWithFutureBirthDate, _edFiVersion);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("future"));
        }

        [Test]
        public async Task ValidateDataObject_ValidLearningStandard_ShouldPass()
        {
            // Arrange
            var validLearningStandard = JObject.FromObject(new
            {
                learningStandardId = "LS-001",
                description = "Students will demonstrate understanding of mathematical concepts",
                academicSubjects = new[]
                {
                    new { academicSubjectDescriptor = "uri://ed-fi.org/AcademicSubjectDescriptor#Mathematics" }
                },
                gradeLevels = new[]
                {
                    new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Third grade" }
                }
            });

            // Act
            var result = await _validator.ValidateDataObjectAsync("learningStandards", validLearningStandard, _edFiVersion);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task ValidateDataObject_LearningStandardMissingAcademicSubjects_ShouldFail()
        {
            // Arrange
            var invalidLearningStandard = JObject.FromObject(new
            {
                learningStandardId = "LS-001",
                description = "Students will demonstrate understanding of mathematical concepts",
                gradeLevels = new[]
                {
                    new { gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Third grade" }
                }
                // Missing academicSubjects
            });

            // Act
            var result = await _validator.ValidateDataObjectAsync("learningStandards", invalidLearningStandard, _edFiVersion);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("academic subject"));
        }

        [Test]
        public void ValidateDescriptors_InvalidDescriptorFormat_ShouldFail()
        {
            // Arrange
            var dataWithInvalidDescriptor = JObject.FromObject(new
            {
                gradeLevel = "Third Grade", // Invalid descriptor format
                academicSubject = "uri://ed-fi.org/AcademicSubjectDescriptor#Mathematics" // Valid
            });

            // Act
            var result = _validator.ValidateDescriptors(dataWithInvalidDescriptor);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("Invalid"));
        }

        [Test]
        public void ValidateDescriptors_ValidDescriptors_ShouldPass()
        {
            // Arrange
            var dataWithValidDescriptors = JObject.FromObject(new
            {
                gradeLevelDescriptor = "uri://ed-fi.org/GradeLevelDescriptor#Third grade",
                academicSubjectDescriptor = "uri://ed-fi.org/AcademicSubjectDescriptor#Mathematics"
            });

            // Act
            var result = _validator.ValidateDescriptors(dataWithValidDescriptors);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public void ValidateRequiredFields_AllFieldsPresent_ShouldPass()
        {
            // Arrange
            var completeStudent = JObject.FromObject(new
            {
                studentUniqueId = "12345",
                firstName = "John",
                lastSurname = "Doe",
                birthDate = "2010-01-15"
            });

            // Act
            var result = _validator.ValidateRequiredFields("students", completeStudent);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public void ValidateDataTypes_StringLengthExceeded_ShouldFail()
        {
            // Arrange
            var studentWithLongName = JObject.FromObject(new
            {
                studentUniqueId = "12345",
                firstName = new string('A', 100), // Too long (max 75)
                lastSurname = "Doe",
                birthDate = "2010-01-15"
            });

            // Act
            var result = _validator.ValidateDataTypes("students", studentWithLongName);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("exceeds maximum length"));
        }

        [Test]
        public async Task ValidateBulkJsonModel_ValidData_ShouldPass()
        {
            // Arrange
            var validBulkModel = new EdFiBulkJsonModel
            {
                Resource = "students",
                Schema = "ed-fi",
                Operation = "Upsert",
                Data = new List<JObject>
                {
                    JObject.FromObject(new
                    {
                        studentUniqueId = "12345",
                        firstName = "John",
                        lastSurname = "Doe",
                        birthDate = "2010-01-15"
                    }),
                    JObject.FromObject(new
                    {
                        studentUniqueId = "67890",
                        firstName = "Jane",
                        lastSurname = "Smith",
                        birthDate = "2011-03-20"
                    })
                }
            };

            // Act
            var result = await _validator.ValidateBulkJsonModelAsync(validBulkModel, _edFiVersion);

            // Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task ValidateBulkJsonModel_InvalidOperation_ShouldFail()
        {
            // Arrange
            var invalidBulkModel = new EdFiBulkJsonModel
            {
                Resource = "students",
                Schema = "ed-fi",
                Operation = "Delete", // Not supported
                Data = new List<JObject>
                {
                    JObject.FromObject(new { studentUniqueId = "12345" })
                }
            };

            // Act
            var result = await _validator.ValidateBulkJsonModelAsync(invalidBulkModel, _edFiVersion);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("Upsert"));
        }

        [Test]
        public async Task ValidateBulkJsonModel_EmptyData_ShouldFail()
        {
            // Arrange
            var emptyBulkModel = new EdFiBulkJsonModel
            {
                Resource = "students",
                Schema = "ed-fi",
                Operation = "Upsert",
                Data = new List<JObject>() // Empty
            };

            // Act
            var result = await _validator.ValidateBulkJsonModelAsync(emptyBulkModel, _edFiVersion);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("empty"));
        }

        [Test]
        public async Task ValidateDataObjects_MixedValidityData_ShouldAggregateErrors()
        {
            // Arrange
            var mixedData = new List<JObject>
            {
                JObject.FromObject(new // Valid
                {
                    studentUniqueId = "12345",
                    firstName = "John",
                    lastSurname = "Doe",
                    birthDate = "2010-01-15"
                }),
                JObject.FromObject(new // Invalid - missing fields
                {
                    firstName = "Jane"
                })
            };

            // Act
            var result = await _validator.ValidateDataObjectsAsync("students", mixedData, _edFiVersion);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("studentUniqueId"));
        }
    }
}
