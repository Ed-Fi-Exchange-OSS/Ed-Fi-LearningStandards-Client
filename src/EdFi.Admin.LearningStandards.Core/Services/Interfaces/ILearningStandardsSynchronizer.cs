// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Configuration;

namespace EdFi.Admin.LearningStandards.Core.Services.Interfaces
{
    public interface ILearningStandardsSynchronizer
    {
        [Obsolete("This method implicitly does a full sync. It is maintained for backwards compatibility and may be removed in a future release.")]
        Task<IResponse> SynchronizeAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IAuthenticationConfiguration learningStandardsAuthenticationConfiguration,
            CancellationToken cancellationToken,
            IProgress<LearningStandardsSynchronizerProgressInfo> progress);

        Task<IResponse> SynchronizeAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            IAuthenticationConfiguration learningStandardsAuthenticationConfiguration,
            ILearningStandardsSynchronizationOptions options,
            CancellationToken cancellationToken,
            IProgress<LearningStandardsSynchronizerProgressInfo> progress);
    }
}
