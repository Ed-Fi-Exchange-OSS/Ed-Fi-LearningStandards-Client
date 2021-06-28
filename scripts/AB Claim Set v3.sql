-- SPDX-License-Identifier: Apache-2.0
-- Licensed to the Ed-Fi Alliance under one or more agreements.
-- The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
-- See the LICENSE and NOTICES files in the project root for more information.

USE [EdFi_Security]
---------------------------------- Learning Standards CLI ---------------------------------------
-- Create AB Vendor ClaimSet

DECLARE @applicationId int
DECLARE @applicationName  nvarchar(max)
DECLARE @claimSetName  nvarchar(255)
SET @claimSetName = 'AB Vendor'
SET @applicationName = 'Ed-Fi ODS API'

SELECT @applicationId = ApplicationId FROM  Applications WHERE ApplicationName = @applicationName

PRINT 'Ensuring AB Vendor Claimset exists.'
INSERT INTO ClaimSets (ClaimSetName, Application_ApplicationId)
SELECT DISTINCT @claimSetName, @applicationId FROM ClaimSets
WHERE NOT EXISTS (SELECT *
                  FROM ClaimSets
				  WHERE ClaimSetName = @claimSetName AND Application_ApplicationId = @applicationId )
GO

-- Configure AB Vendor ClaimSet

DECLARE @actionName nvarchar(255)
DECLARE @claimSetName nvarchar(255)
DECLARE @resourceNames TABLE (ResourceName nvarchar(255))
DECLARE @resourceClaimIds TABLE (ResourceClaimId int)

SET @claimSetName = 'AB Vendor'
PRINT 'Creating temorary records.'
INSERT INTO @resourceNames VALUES ('gradeLevelDescriptor'),('academicSubjectDescriptor'),('publicationStatusDescriptor'),('educationStandards')
INSERT INTO @resourceClaimIds SELECT ResourceClaimId FROM ResourceClaims WHERE ResourceName IN (SELECT ResourceName FROM @resourceNames)

DECLARE @actionId int
DECLARE @claimSetId int

SELECT @actionId = ActionId FROM Actions WHERE ActionName = @actionName
SELECT @claimSetId = ClaimSetId FROM ClaimSets WHERE ClaimSetName = @claimSetName

PRINT 'Configuring Claims for AB Vendor Claimset...'
INSERT INTO ClaimSetResourceClaims
    (Action_ActionId, ClaimSet_ClaimSetId, ResourceClaim_ResourceClaimId)
SELECT ActionId, @claimSetId, ResourceClaimId FROM Actions a, @resourceClaimIds rc
WHERE NOT EXISTS (SELECT *
                  FROM ClaimSetResourceClaims
				  WHERE Action_ActionId = a.ActionId AND ClaimSet_ClaimSetId = @claimSetId AND ResourceClaimId = rc.ResourceClaimId)
GO
--------------------------------------------------------------------------------------------------

--------------------------------- Learning Standards Use -----------------------------------------

-- Shift educationStandards to not require further authorization for READ (previously was typically Namespace restricted)
-- This allows claimsets that have a READ permission on educationStandards to read learning standards loaded in to other
-- namespaces (Like AB)

DECLARE @authorizationStrategyId INT
DECLARE @resourceClaimId INT
DECLARE @actionId INT

SELECT @authorizationStrategyId = AuthorizationStrategyId
FROM   AuthorizationStrategies
WHERE  AuthorizationStrategyName = 'NoFurtherAuthorizationRequired'

SELECT @resourceClaimId = ResourceClaimId
FROM   ResourceClaims
WHERE  ResourceName = 'educationStandards'

SELECT @ActionId = ActionId FROM Actions WHERE ActionName = 'Read'

PRINT 'Updating educationStandards authorization strategy for READ.'

UPDATE ResourceClaimAuthorizationMetadatas SET AuthorizationStrategy_AuthorizationStrategyId = @authorizationStrategyId
WHERE  Action_ActionId = @actionId AND ResourceClaim_ResourceClaimId = @resourceClaimId

GO

-- Add managed descriptors to the assessment vendor claimset (if not already present)

DECLARE @managedDescriptorClaimId as INT

SELECT @managedDescriptorClaimId = ResourceClaimId
FROM dbo.ResourceClaims rc
WHERE rc.ResourceName = 'managedDescriptors'

DECLARE @claimSetId as INT

SELECT @claimSetId = ClaimsetId
FROM dbo.ClaimSets c
WHERE c.ClaimSetName = 'Assessment Vendor'

PRINT 'Ensuring all actions for managedDescriptors are assigned to Assessment Vendor claimset'
-- Add all actions for managed descriptors.
INSERT INTO ClaimSetResourceClaims
    (Action_ActionId, ClaimSet_ClaimSetId, ResourceClaim_ResourceClaimId)
SELECT ActionId, @claimSetId, @managedDescriptorClaimId FROM Actions a
WHERE NOT EXISTS (SELECT *
                  FROM ClaimSetResourceClaims
				  WHERE Action_ActionId = a.ActionId AND ClaimSet_ClaimSetId = @claimSetId AND ClaimsetResourceClaims.ResourceClaim_ResourceClaimId = @managedDescriptorClaimId)
GO
----------------------------------------------------------------------------------------------------
