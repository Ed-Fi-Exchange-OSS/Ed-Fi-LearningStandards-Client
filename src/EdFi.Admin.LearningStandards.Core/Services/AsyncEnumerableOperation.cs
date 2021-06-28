// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Async;

namespace EdFi.Admin.LearningStandards.Core.Services
{
    public class AsyncEnumerableOperation<TEntity>
        where TEntity : class
    {
        public AsyncEnumerableOperation()
        {
        }

        public AsyncEnumerableOperation(Guid processId, IAsyncEnumerable<TEntity> asyncEntityEnumerable)
        {
            ProcessId = processId;
            AsyncEntityEnumerable = asyncEntityEnumerable;
        }

        public Guid ProcessId { get; set; }

        public IAsyncEnumerable<TEntity> AsyncEntityEnumerable { get; set; }
    }
}
