// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Auth;
using EdFi.Admin.LearningStandards.Core.Models;

namespace EdFi.Admin.LearningStandards.Core.Services.Interfaces
{
    public interface ILearningStandardsDataRetriever
    {
        AsyncEnumerableOperation<EdFiBulkJsonModel> GetLearningStandardsDescriptors(
            EdFiOdsApiCompatibilityVersion version,
            IChangeSequence syncStartSequence,
            IAuthTokenManager learningStandardsProviderAuthTokenManager,
            CancellationToken cancellationToken = default);

        AsyncEnumerableOperation<EdFiBulkJsonModel> GetLearningStandards(
            EdFiOdsApiCompatibilityVersion version,
            IChangeSequence syncStartSequence,
            IAuthTokenManager learningStandardsProviderAuthTokenManager,
            CancellationToken cancellationToken = default);

        Task<IChangesAvailableResponse> GetChangesAsync(
            IChangeSequence currentSequence,
            IAuthTokenManager learningStandardsProviderAuthTokenManager,
            CancellationToken cancellationToken = default);

        event EventHandler<AsyncEnumerableOperationStatus> ProcessCountEvent;
    }
}
