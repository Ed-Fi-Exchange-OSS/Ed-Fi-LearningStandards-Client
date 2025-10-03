// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Net;

namespace EdFi.Admin.LearningStandards.Core.Models
{
    /// <summary>
    /// Represents detailed validation results for Ed-Fi data objects
    /// </summary>
    public class EdFiValidationResult : IResponse
    {
        public EdFiValidationResult(bool isSuccess, string resourceType, int itemIndex = -1)
        {
            IsSuccess = isSuccess;
            ResourceType = resourceType;
            ItemIndex = itemIndex;
            ValidationErrors = new List<ValidationError>();
            InnerResponses = new List<IResponse>();
        }

        public bool IsSuccess { get; private set; }
        public string ResourceType { get; }
        public int ItemIndex { get; }
        public List<ValidationError> ValidationErrors { get; }

        public string ErrorMessage => ValidationErrors.Count > 0
            ? $"Validation failed for {ResourceType}: {string.Join("; ", ValidationErrors)}"
            : string.Empty;

        public string Content => IsSuccess
            ? $"Validation passed for {ResourceType}"
            : ErrorMessage;

        public HttpStatusCode StatusCode => IsSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        public List<IResponse> InnerResponses { get; }

        public void AddError(string field, string message, ValidationErrorType errorType = ValidationErrorType.General)
        {
            ValidationErrors.Add(new ValidationError(field, message, errorType));
            IsSuccess = false;
        }

        public void AddErrors(IEnumerable<ValidationError> errors)
        {
            ValidationErrors.AddRange(errors);
            if (ValidationErrors.Count > 0)
                IsSuccess = false;
        }

        public override string ToString()
        {
            return IsSuccess ? Content : ErrorMessage;
        }
    }

    /// <summary>
    /// Represents a single validation error
    /// </summary>
    public class ValidationError
    {
        public ValidationError(string field, string message, ValidationErrorType errorType = ValidationErrorType.General)
        {
            Field = field;
            Message = message;
            ErrorType = errorType;
        }

        public string Field { get; }
        public string Message { get; }
        public ValidationErrorType ErrorType { get; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Field) ? Message : $"{Field}: {Message}";
        }
    }

    /// <summary>
    /// Types of validation errors
    /// </summary>
    public enum ValidationErrorType
    {
        General,
        RequiredField,
        DataType,
        Format,
        BusinessRule,
        Descriptor,
        Length,
        Range
    }
}
