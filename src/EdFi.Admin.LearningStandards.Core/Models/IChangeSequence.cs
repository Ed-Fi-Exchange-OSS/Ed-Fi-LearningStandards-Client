// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace EdFi.Admin.LearningStandards.Core.Models
{
    public interface IChangeSequence
    {
        long Id { get; set; }
        IChangeSequenceKey Key { get; set; }
    }

    public interface IChangeSequenceKey
        : IEquatable<IChangeSequenceKey>
    {
        string EdFiApiKey { get; }
        string LearningStandardCredentialId { get; }
    }
}
