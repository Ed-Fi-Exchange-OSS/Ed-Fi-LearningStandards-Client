-- SPDX-License-Identifier: Apache-2.0
-- Licensed to the Ed-Fi Alliance under one or more agreements.
-- The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
-- See the LICENSE and NOTICES files in the project root for more information.

USE EdFi_Admin
GO

DECLARE @VendorName varchar(150)
DECLARE @VendorId int
DECLARE @UserId int
DECLARE @ApplicationId int
DECLARE @FullName varchar(150)
DECLARE @EmailAddress varchar(150)
DECLARE @EducationOrganizationId int
DECLARE @ApplicationEducationOrganizationId int
DECLARE @ClaimSetName nvarchar(255)
DECLARE @VendorNamespace nvarchar(255)
DECLARE @ApplicationName nvarchar(255)

SET @VendorName = 'Certica Test Vendor'
SET @EducationOrganizationId = '101000'
SET @ApplicationName = 'Certia App - GrandBend'
--NOTE if vendor already exists (by VendorName) these values are NOT used
SET @ClaimSetName = 'AB Vendor'
SET @VendorNamespace = 'http://academicbenchmarks.com/'
SET @FullName = 'R. McDonald'
SET @EmailAddress = 'rmcdonald@certicasolutions.com'


SELECT @VendorId = VendorId FROM Vendors WHERE VendorName = @VendorName
IF (@VendorId IS NULL)
	BEGIN
		INSERT INTO Vendors (VendorName,NamespacePrefix)
		VALUES (@VendorName,@VendorNamespace)
		SET @VendorId = @@IDENTITY
		INSERT INTO Users (Email, FullName, Vendor_VendorId)
		VALUES (@EmailAddress, @FullName, @VendorId)
		SET @UserId = @@IDENTITY
	END
ELSE
	BEGIN
		SELECT TOP 1 @UserId = UserId
		FROM Users
		WHERE Vendor_VendorId = @VendorId
	END

SELECT TOP 1 @ApplicationId = ApplicationId FROM Applications WHERE Vendor_VendorId = @VendorId and ApplicationName = @ApplicationName
IF (@ApplicationId IS NULL)
    BEGIN
		INSERT INTO Applications (ApplicationName, Vendor_VendorId, ClaimSetName)
		VALUES (@ApplicationName, @VendorId, @ClaimSetName)
		SET @ApplicationId = @@IDENTITY
	END

PRINT @VendorId
PRINT @ApplicationId
PRINT @UserId

INSERT INTO ApplicationEducationOrganizations (EducationOrganizationId, Application_ApplicationId)
VALUES (@EducationOrganizationId, @ApplicationId)
SET @ApplicationEducationOrganizationId = @@IDENTITY
DECLARE @Key varchar(20)
DECLARE @Secret varchar(15)
DECLARE @ApiClientId int
SET @Key = REPLACE(SUBSTRING(CAST(NEWID() AS varchar(50)), 0, 20), '-', '')
SET @Secret = REPLACE(SUBSTRING(CAST(NEWID() AS varchar(50)), 0, 15), '-', '')
INSERT INTO ApiClients ([Key], Secret, Name, IsApproved, UseSandbox, SandboxType, Application_ApplicationId, User_UserId)
VALUES (@Key, @Secret, @VendorName, 1, 0, 0, @ApplicationId, @UserId)
SET @ApiClientId = @@IDENTITY
INSERT INTO ApiClientApplicationEducationOrganizations (ApplicationEducationOrganization_ApplicationEducationOrganizationId, ApiClient_ApiClientId)
VALUES (@ApplicationEducationOrganizationId, @ApiClientId)

PRINT @Key
PRINT @Secret
