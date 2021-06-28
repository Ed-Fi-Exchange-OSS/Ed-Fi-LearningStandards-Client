---------------------------------- Learning Standards CLI ---------------------------------------
DO $$
    DECLARE application_name varchar(50) := 'Ed-Fi ODS API';
    DECLARE application_id int;
    DECLARE claimset_name varchar(50) := 'AB Vendor';
    DECLARE claimset_id int;
BEGIN

    IF NOT EXISTS (SELECT 1 FROM dbo.applications WHERE applicationname = application_name)
    THEN
        RAISE NOTICE '% does not exist', application_name;
    END IF;

    SELECT applicationid INTO application_id
    FROM dbo.applications
    WHERE applicationname = application_name;

    -- Create AB Vendor ClaimSet
    IF EXISTS (SELECT 1 FROM dbo.claimsets WHERE claimsetname = claimset_name)
    THEN
        RAISE NOTICE '% claimset exists', claimset_name;
    ELSE
        RAISE NOTICE 'adding % claimset', claimset_name;
        INSERT INTO dbo.claimsets (claimsetname, application_applicationid) VALUES (claimset_name, application_id);
    END IF;

    SELECT claimsetid INTO claimset_id
    FROM dbo.claimsets
    WHERE claimsetname = claimset_name;

    -- Configure AB Vendor ClaimSet
    IF EXISTS (SELECT 1 FROM dbo.claimsetresourceclaims WHERE claimset_claimsetid = claimset_id)
    THEN
        RAISE NOTICE 'claims already exist for claim %', claimset_name;
    ELSE
        RAISE NOTICE 'Configuring Claims for % Claimset...', claimset_name;
        INSERT INTO dbo.claimsetresourceclaims
            (Action_ActionId
            ,ClaimSet_ClaimSetId
            ,ResourceClaim_ResourceClaimId
            ,AuthorizationStrategyOverride_AuthorizationStrategyId
            ,ValidationRuleSetNameOverride)
        SELECT ac.actionid, claimset_id, resourceclaimid, CAST(null AS int), CAST(null AS int)
        FROM dbo.resourceclaims
        INNER JOIN LATERAL
            (SELECT actionid
            FROM dbo.actions
            WHERE actionname in ('Create','Read','Update','Delete')) AS ac ON true
        WHERE resourcename IN ('gradeLevelDescriptor','academicSubjectDescriptor','publicationStatusDescriptor','educationStandards');
    END IF;
END $$;

--------------------------------- Learning Standards Use -----------------------------------------
/*
    Shift educationStandards to not require further authorization for READ (previously was typically Namespace restricted)
    This allows claimsets that have a READ permission on educationStandards to read learning standards loaded in to other
    namespaces (Like AB)
*/
DO $$
    DECLARE authorization_strategy_id int;
    DECLARE resource_claim_id int;
    DECLARE action_id int;
BEGIN
    SELECT authorizationstrategyid INTO authorization_strategy_id
    FROM dbo.authorizationstrategies
    WHERE authorizationstrategyname = 'NoFurtherAuthorizationRequired';

    SELECT resourceclaimid INTO resource_claim_id
    FROM dbo.resourceclaims
    WHERE resourcename = 'educationStandards';

    SELECT actionid INTO action_id
    FROM dbo.actions
    WHERE actionname = 'Read';

    RAISE NOTICE 'Updating educationStandards authorization strategy for READ.';

    UPDATE dbo.resourceclaimauthorizationmetadatas
    SET authorizationstrategy_authorizationstrategyid = authorization_strategy_id
    WHERE action_actionid = action_id AND resourceclaim_resourceclaimid = resource_claim_id;
END $$;

-- Add managed descriptors to the assessment vendor claimset (if not already present)
DO $$
    DECLARE resourceclaim_id int;
    DECLARE claimset_id int;
BEGIN
    SELECT resourceclaimid into resourceclaim_id
    FROM dbo.resourceclaims
    WHERE resourcename = 'managedDescriptors';

    SELECT claimsetid into claimset_id
    FROM dbo.claimsets
    WHERE claimsetname = 'Assessment Vendor';

    IF EXISTS (SELECT 1
               FROM dbo.claimsetresourceclaims
               WHERE claimset_claimsetid = claimset_id AND resourceclaim_resourceclaimid = resourceclaim_id)
    THEN
        RAISE NOTICE 'managedDescriptors claim already exist for Assessment Vendor';
    ELSE
        RAISE NOTICE 'Ensuring all actions for managedDescriptors are assigned to Assessment Vendor claimset';
        INSERT INTO dbo.claimsetresourceclaims
            (Action_ActionId
            ,ClaimSet_ClaimSetId
            ,ResourceClaim_ResourceClaimId
            ,AuthorizationStrategyOverride_AuthorizationStrategyId
            ,ValidationRuleSetNameOverride)
        SELECT ac.actionid, claimset_id, resourceclaimid, CAST(null AS int), CAST(null AS int)
        FROM dbo.resourceclaims
        INNER JOIN LATERAL
            (SELECT actionid
            FROM dbo.actions
            WHERE actionname IN ('Create','Read','Update','Delete')) AS ac ON true
        WHERE resourceclaimid = resourceclaim_id;
    END IF;
END $$;
