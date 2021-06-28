// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace EdFi.Admin.LearningStandards.Core
{
    public class EdFiBulkJsonModel
    {
        public string Schema { get; set; }
        public string Resource { get; set; }
        public string Operation { get; set; }
        public IList<JObject> Data { get; set; }
    }
}
