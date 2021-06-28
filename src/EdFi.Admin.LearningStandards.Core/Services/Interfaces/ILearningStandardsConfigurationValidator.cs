// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Configuration;

namespace EdFi.Admin.LearningStandards.Core.Services.Interfaces
{
    /// <summary>
    /// Allows for configuration objects to be validated before being used with the Learning Standard Synchronization process
    /// </summary>
    public interface ILearningStandardsConfigurationValidator
    {
        /// <summary>
        /// Allows for both <see cref="IAuthenticationConfiguration"/> and <see cref="IEdFiOdsApiConfiguration"/> objects
        /// to be validated before being used with the Learning Standard Synchronization process
        /// </summary>
        /// <param name="learningStandardsAuthenticationConfiguration">The <see cref="IAuthenticationConfiguration"/> to validate</param>
        /// <param name="edFiOdsApiConfiguration">The <see cref="IEdFiOdsApiConfiguration"/> to validate</param>
        /// <returns>An <see cref="IResponse"/> entity containing the validation results</returns>
        Task<IResponse> ValidateConfigurationAsync(IAuthenticationConfiguration learningStandardsAuthenticationConfiguration, IEdFiOdsApiConfiguration edFiOdsApiConfiguration);

        /// <summary>
        /// Allows for an <see cref="IAuthenticationConfiguration"/> to be validated before being used with the Learning Standard Synchronization process
        /// </summary>
        /// <param name="learningStandardsAuthenticationConfiguration">The <see cref="IAuthenticationConfiguration"/> to validate</param>
        /// <returns>An <see cref="IResponse"/> entity containing the validation results</returns>
        Task<IResponse> ValidateLearningStandardProviderConfigurationAsync(IAuthenticationConfiguration learningStandardsAuthenticationConfiguration);

        /// <summary>
        /// Allows for an <see cref="IEdFiOdsApiConfiguration"/> to be validated before being used with the Learning Standard Synchronization process
        /// </summary>
        /// <param name="edFiOdsApiConfiguration">The <see cref="IEdFiOdsApiConfiguration"/> to validate</param>
        /// <returns>An <see cref="IResponse"/> entity containing the validation results</returns>
        Task<IResponse> ValidateEdFiOdsApiConfigurationAsync(IEdFiOdsApiConfiguration edFiOdsApiConfiguration);
    }
}
