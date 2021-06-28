// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace EdFi.Admin.LearningStandards.Core
{
    public class LearningStandardsSynchronizerProgressInfo
    {
        public string TaskName { get; }
        public string TaskState { get; }
        public int CompletedPercentage { get; }

        public LearningStandardsSynchronizerProgressInfo(string taskName, string taskState, int completedPercentage)
        {
            TaskName = taskName;
            TaskState = taskState;
            CompletedPercentage = completedPercentage;
        }

        public LearningStandardsSynchronizerProgressInfo(string taskName, int completedPercentage, Exception exception) : this(taskName,$"Error:{exception.Message}",completedPercentage)
        {

        }
    }
}
