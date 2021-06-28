// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.Admin.LearningStandards.Core.Configuration
{
    public class AuthenticationConfiguration : IAuthenticationConfiguration
    {
        /// <summary>
        /// Authentication configuration
        /// </summary>
        /// <param name="key">The key to use for authentication</param>
        /// <param name="secret">The secret to use for authentication</param>
        public AuthenticationConfiguration(string key, string secret)
        {
            Key = key;
            Secret = secret;
        }

        public string Key { get; }

        public string Secret { get; }
    }
}
