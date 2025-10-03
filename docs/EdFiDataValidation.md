# Ed-Fi Data Validation System

## Overview

The Ed-Fi Data Validation System provides comprehensive pre-submission validation for all Ed-Fi API data objects. This system validates data against Ed-Fi 4.0 Data Standard specifications before sending requests to the API, helping prevent common validation errors and improving data quality.

## Features

### Pre-Submission Validation

- **Required Fields Validation**: Ensures all mandatory fields are present and non-empty
- **Data Type Validation**: Validates field types, formats, and ranges
- **Descriptor Validation**: Validates Ed-Fi descriptor URIs are properly formatted
- **Business Rule Validation**: Enforces Ed-Fi-specific business logic
- **String Length Validation**: Ensures strings don't exceed Ed-Fi field limits
- **Date Validation**: Validates date formats and logical date ranges

### Error Handling & Reporting

- **Detailed Error Messages**: Provides specific field-level validation errors
- **Ed-Fi API Error Parsing**: Extracts and interprets Ed-Fi API validation responses
- **Aggregated Validation Results**: Collects all validation errors for comprehensive reporting
- **Correlation ID Tracking**: Includes Ed-Fi API correlation IDs for debugging

### Supported Resource Types

- Students
- Learning Standards
- Schools
- Staff
- Courses
- Sections
- Assessments
- Student Assessments
- Descriptors (Academic Subject, Grade Level, Publication Status)

## Quick Start

### 1. Enable Validation in Startup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add Ed-Fi validation services
    services.AddEdFiDataValidation();

    // Configure validation options (optional)
    services.Configure<EdFiValidationOptions>(options =>
    {
        options.EnablePreValidation = true;
        options.ValidateDescriptors = true;
        options.ValidateBusinessRules = true;
    });
}
```

### 2. Validation is Automatic

The validation system integrates seamlessly with the existing `EdFiBulkJsonPersister`. All data is automatically validated before submission:

```csharp
// Your existing code - validation happens automatically
var results = await bulkJsonPersister.PostEdFiBulkJson(edFiBulkJson);

// Check for validation errors
foreach (var result in results)
{
    if (!result.IsSuccess)
    {
        Console.WriteLine($"Validation Error: {result.ErrorMessage}");
    }
}
```

### 3. Manual Validation (Optional)

You can also perform validation manually:

```csharp
public class MyService
{
    private readonly IEdFiDataValidator _validator;

    public MyService(IEdFiDataValidator validator)
    {
        _validator = validator;
    }

    public async Task<bool> ValidateStudentData(JObject studentData)
    {
        var edFiVersion = new EdFiVersionModel(/* ... */);
        var result = await _validator.ValidateDataObjectAsync("students", studentData, edFiVersion);

        if (!result.IsSuccess)
        {
            // Handle validation errors
            Console.WriteLine($"Validation failed: {result.ErrorMessage}");
            return false;
        }

        return true;
    }
}
```

## Validation Rules

### Required Fields by Resource Type

#### Students

- `studentUniqueId`
- `firstName`
- `lastSurname`
- `birthDate`

#### Learning Standards

- `learningStandardId`
- `description`
- `academicSubjects` (at least one)
- `gradeLevels` (at least one)

#### Schools

- `schoolId`
- `nameOfInstitution`

### Data Type Validation

#### Dates

- Must be in `YYYY-MM-DD` format
- Birth dates cannot be in the future
- Assessment dates cannot be in the future
- Reasonable age ranges (not over 150 years old)

#### Strings

- Field length limits enforced per Ed-Fi specifications
- Examples:
  - `firstName`, `lastSurname`: 75 characters max
  - `description`: 1024 characters max
  - `courseTitle`: 60 characters max

#### Descriptors

- Must follow URI pattern: `uri://domain/DescriptorType#Value`
- Examples:
  - `uri://ed-fi.org/GradeLevelDescriptor#Third grade`
  - `uri://ed-fi.org/AcademicSubjectDescriptor#Mathematics`

### Business Rules

#### Students

- Birth date must be in the past
- Grade levels must be valid Ed-Fi standard values
- Age must be reasonable (0-150 years)

#### Learning Standards

- Description must be at least 10 characters
- Must have at least one academic subject
- Must have at least one grade level
- Course title cannot exceed 60 characters

#### Assessments

- Administration date cannot be in the future
- Score values must be non-negative
- Score values must be reasonable (< 9999)

## Error Response Format

### Validation Errors

```json
{
  "isSuccess": false,
  "errorMessage": "Validation failed for students: Required field 'studentUniqueId' is missing or empty; Field 'birthDate' must be in YYYY-MM-DD format",
  "statusCode": 400
}
```

### Ed-Fi API Errors

```json
{
  "isSuccess": false,
  "errorMessage": "Ed-Fi API Validation Errors: Field 'birthDate': The supplied value is invalid.; Field 'firstName': This field is required.",
  "statusCode": 400,
  "content": "{\"detail\":\"Data validation failed.\",\"correlationId\":\"abc123\"}"
}
```

## Configuration Options

### EdFiValidationOptions

```csharp
services.Configure<EdFiValidationOptions>(options =>
{
    options.EnablePreValidation = true;        // Enable/disable pre-validation
    options.FailFast = false;                  // Collect all errors vs. fail on first
    options.MaxErrorsPerObject = 100;          // Max errors to collect per object
    options.ValidateDescriptors = true;        // Validate descriptor formats
    options.ValidateBusinessRules = true;      // Validate business logic
    options.ValidateStringLengths = true;      // Validate string lengths
    options.ValidateDates = true;              // Validate date formats/ranges
    options.IncludeDetailedContext = true;     // Include context in errors

    // Custom validation rules
    options.CustomRequiredFields["customResource"] = new[] { "field1", "field2" };
    options.CustomStringLengthLimits["customField"] = 200;
});
```

### EdFiErrorHandlingOptions

```csharp
services.Configure<EdFiErrorHandlingOptions>(options =>
{
    options.RetryOnValidationErrors = false;   // Retry on validation errors
    options.MaxRetries = 3;                    // Max retry attempts
    options.RetryDelayMs = 1000;              // Delay between retries
    options.LogDetailedErrors = true;          // Log detailed error info
    options.ParseApiErrorResponses = true;     // Parse Ed-Fi API errors
});
```

## Best Practices

### 1. Validate Early and Often

- Validation happens automatically before API submission
- Consider adding validation at data entry points
- Validate after data transformations

### 2. Handle Validation Errors Gracefully

```csharp
var results = await persister.PostEdFiBulkJson(bulkData);

foreach (var result in results)
{
    if (!result.IsSuccess)
    {
        // Log the error
        logger.LogError("Data validation failed: {Error}", result.ErrorMessage);

        // Take corrective action
        await HandleValidationError(result);

        // Continue with next item or abort based on business logic
    }
}
```

### 3. Use Meaningful Error Messages

The validation system provides detailed, field-specific error messages. Use these to:

- Guide users in fixing data issues
- Log specific problems for debugging
- Provide actionable feedback

### 4. Configure Validation Based on Environment

```csharp
// Development: Strict validation with detailed errors
services.Configure<EdFiValidationOptions>(options =>
{
    options.EnablePreValidation = true;
    options.FailFast = false;
    options.IncludeDetailedContext = true;
});

// Production: Performance-optimized validation
services.Configure<EdFiValidationOptions>(options =>
{
    options.EnablePreValidation = true;
    options.FailFast = true;
    options.MaxErrorsPerObject = 10;
});
```

## Troubleshooting

### Common Validation Errors

1. **"Required field 'X' is missing or empty"**
   - Ensure all required fields are populated
   - Check for null or whitespace-only values

2. **"Field 'birthDate' must be in YYYY-MM-DD format"**
   - Use ISO 8601 date format: `2010-01-15`
   - Avoid regional date formats like `01/15/2010`

3. **"Descriptor 'X' has invalid URI format"**
   - Use proper Ed-Fi descriptor format: `uri://domain/DescriptorType#Value`
   - Verify the descriptor namespace and value

4. **"Field 'X' exceeds maximum length"**
   - Check Ed-Fi field length limits
   - Truncate or abbreviate long values appropriately

### Debugging Tips

1. **Enable Detailed Logging**

   ```csharp
   services.Configure<EdFiErrorHandlingOptions>(options =>
   {
       options.LogDetailedErrors = true;
   });
   ```

2. **Check Correlation IDs**
   - Ed-Fi API errors include correlation IDs for tracking
   - Use these when contacting Ed-Fi support

3. **Validate Sample Data**

   ```csharp
   var validator = serviceProvider.GetService<IEdFiDataValidator>();
   var result = await validator.ValidateDataObjectAsync("students", sampleData, edFiVersion);
   Console.WriteLine(result.ErrorMessage);
   ```

## Performance Considerations

- Validation adds processing time before API calls
- Use `FailFast = true` for performance-critical scenarios
- Consider async validation for large datasets
- Monitor validation performance in production

## Extending the Validation System

### Adding Custom Resource Types

1. Add required fields to `RequiredFieldsMap` in `EdFiDataValidator`
2. Add string length limits to `StringLengthLimits`
3. Implement custom business rules in `ValidateBusinessRules`

### Adding Custom Validation Rules

```csharp
services.Configure<EdFiValidationOptions>(options =>
{
    options.CustomRequiredFields["myResource"] = new[] { "field1", "field2" };
    options.CustomStringLengthLimits["myField"] = 500;
});
```

This validation system ensures high data quality and reduces Ed-Fi API errors, making your Ed-Fi integrations more reliable and maintainable.
