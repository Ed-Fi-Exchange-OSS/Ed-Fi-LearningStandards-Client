// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using CommandLine;

namespace EdFi.Admin.LearningStandards.CLI
{
    [Verb("changes", HelpText = "Determines whether changes to learnings standards exist for synchronization into the specified Ed-Fi ODS API.")]
    public class LearningStandardsCLIChangesOptions : LearningStandardsCLIBaseOptions
    {
        [Option('o', "output", Required = false, HelpText = "Set the output format to one of two options: <text:default>|<json>.")]
        public string OutputFormat { get; set; } = "text";
    }
}
