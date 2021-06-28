// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace EdFi.Admin.LearningStandards.Core.Configuration
{
    public interface ILearningStandardsProviderConfiguration
    {
        string Url { get; }

        int Retries { get; }

        int MaxSimultaneousRequests { get; }

        int AuthorizationWindowSeconds { get; }

        int DefaultReportedRecordCount { get; set; }
    }
}
