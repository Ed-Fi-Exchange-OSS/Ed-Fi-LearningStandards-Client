// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace EdFi.Admin.LearningStandards.CLI.Utilities
{
    public class DictionaryAsArrayResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            return objectType.GetInterfaces()
                             .Any(i => i == typeof(IDictionary) ||
                                       (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                ? base.CreateArrayContract(objectType)
                : base.CreateContract(objectType);
        }
    }
}
