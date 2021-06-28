// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using Newtonsoft.Json;

namespace EdFi.Admin.LearningStandards.Core.Models
{
    [JsonObject]
    public class ChangeSequence : IChangeSequence
    {
        [JsonProperty]
        public long Id { get; set; }

        [JsonProperty, JsonRequired]
        public IChangeSequenceKey Key { get; set; }
    }

    [JsonObject]
    public class ChangeSequenceKey : IChangeSequenceKey
    {
        [JsonProperty, JsonRequired]
        public string EdFiApiKey { get; }

        [JsonProperty, JsonRequired]
        public string LearningStandardCredentialId { get; }

        public ChangeSequenceKey(string edFiApiKey, string learningStandardCredentialId)
        {
            EdFiApiKey = edFiApiKey;
            LearningStandardCredentialId = learningStandardCredentialId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as ChangeSequenceKey);
        }
        public bool Equals(IChangeSequenceKey obj)
        {
            return obj != null
                   && obj.LearningStandardCredentialId.Equals(
                       LearningStandardCredentialId,
                       StringComparison.InvariantCultureIgnoreCase)
                   && obj.EdFiApiKey.Equals(
                       EdFiApiKey,
                       StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * 23 + LearningStandardCredentialId.GetHashCode();
                hashCode = hashCode * 23 + EdFiApiKey.GetHashCode();

                return hashCode;
            }
        }
    }
}
