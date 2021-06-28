// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.Admin.LearningStandards.Core
{
    public static class LearningStandardsResponseExtensions
    {
        public static IEnumerable<IResponse> Flatten(this IEnumerable<IResponse> responses)
        {
            return responses.Flatten(f => f.InnerResponses).Select(sl => new ResponseModel(sl.IsSuccess, sl.ErrorMessage, sl.Content, sl.StatusCode));
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> original, Func<T, IEnumerable<T>> target)
        {
            var enumerated = original.ToList();
            return enumerated.SelectMany(c => target(c).Flatten(target)).Concat(enumerated);
        }
    }
}
