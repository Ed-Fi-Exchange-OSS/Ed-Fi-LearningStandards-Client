// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using CommandLine;

namespace EdFi.Admin.LearningStandards.CLI
{
    [Verb("sync", HelpText = "Synchronizes learning standards, loading them into the specified Ed-Fi ODS API.")]
    public class LearningStandardsCLISyncOptions : LearningStandardsCLIBaseOptions
    {
        [Option("force-full", Required = false, HelpText = "Instructs the CLI to force a full synchronization, ignoring the current change state and replacing it.")]
        public bool ForceFullSync { get; set; }
    }
}
