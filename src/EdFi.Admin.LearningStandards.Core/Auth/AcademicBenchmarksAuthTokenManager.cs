// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace EdFi.Admin.LearningStandards.Core.Auth
{
    public class AcademicBenchmarksAuthTokenManager : IAuthTokenManager
    {
        private readonly IAuthenticationConfiguration _authenticationConfiguration;

        private readonly ILearningStandardsProviderConfiguration _learningStandardsProviderConfiguration;

        private readonly ILogger<AcademicBenchmarksAuthTokenManager> _logger;

        private const int DefaultTimeWindow = 300;

        private string _token;

        private long _unixUtcExpiration;

        public AcademicBenchmarksAuthTokenManager(
            IOptionsSnapshot<AcademicBenchmarksOptions> academicBenchmarksOptions,
            IAuthenticationConfiguration authenticationConfiguration,
            ILogger<AcademicBenchmarksAuthTokenManager> logger)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(authenticationConfiguration, nameof(authenticationConfiguration));

            _learningStandardsProviderConfiguration = academicBenchmarksOptions?.Value;
            _authenticationConfiguration = authenticationConfiguration;
            _logger = logger;
        }

        /// <summary>
        ///     Retrieves a base64 encoded token containing authentication information needed for AB-Connect
        /// </summary>
        /// <returns>base64 encoded token string</returns>
        public Task<string> GetTokenAsync()
        {
            //If there is a token, and there are at least 5min left to expiration, use it.
            if (!string.IsNullOrWhiteSpace(_token) && _unixUtcExpiration > GetUnixTime(
                    DateTime.UtcNow.AddSeconds(
                        _learningStandardsProviderConfiguration?.AuthorizationWindowSeconds
                        ?? DefaultTimeWindow)))
            {
                _logger.LogDebug("Cache hit: ab-auth-token");
                return Task.FromResult(_token);
            }

            _unixUtcExpiration = GetUnixTime(DateTime.UtcNow.AddHours(24));
            string partnerId = _authenticationConfiguration.Key;
            string partnerKey = _authenticationConfiguration.Secret;
            string sig = CreateSignature(partnerKey, _unixUtcExpiration);

            _token = CreateBase64Token(partnerId, _unixUtcExpiration, sig);

            _logger.LogDebug($"Created token for {partnerId}, expiring {_unixUtcExpiration}");

            return Task.FromResult(_token);
        }

        /// <summary>
        ///     Creates a base64 encoded HMACSHA256 signature
        /// </summary>
        /// <param name="partnerKey">The AB-Connect partner key</param>
        /// <param name="unixExpiration">The token expiration in UNIX time</param>
        /// <returns>base64 encoded HMACSHA256 string</returns>
        private string CreateSignature(string partnerKey, long unixExpiration)
        {
            var keyBytes = Encoding.UTF8.GetBytes(partnerKey);
            var messageBytes = Encoding.UTF8.GetBytes(unixExpiration.ToString());

            string signature;
            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                signature = Convert.ToBase64String(hmac.ComputeHash(messageBytes));
            }

            return signature;
        }

        /// <summary>
        ///     Creates a new token based on the specified parameters
        /// </summary>
        /// <param name="partnerId">The AB-Connect partner id</param>
        /// <param name="expiration">The token expiration in UNIX time</param>
        /// <param name="signature">The base64 encoded HMACSHA256 signature</param>
        /// <returns></returns>
        private string CreateBase64Token(string partnerId, long expiration, string signature)
        {
            Check.NotEmpty(partnerId, nameof(partnerId));
            Check.NotEmpty(signature, nameof(signature));

            JObject j = new JObject
            {
                ["auth"] = new JObject
                {
                    ["partner.id"] = partnerId,
                    ["auth.expires"] = expiration,
                    ["auth.signature"] = signature
                }
            };
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(j.ToString()));
        }

        private long GetUnixTime(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }
    }
}
