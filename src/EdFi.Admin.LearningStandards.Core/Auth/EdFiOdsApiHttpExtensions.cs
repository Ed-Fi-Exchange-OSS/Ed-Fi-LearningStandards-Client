// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core;
using Newtonsoft.Json.Linq;

namespace System.Net.Http
{
    public static class EdFiOdsApiHttpExtensions
    {
        /// <summary>
        /// Serialize the HTTP content to a <see cref="Newtonsoft.Json.Linq.JToken"/> as an asynchronous operation.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static async Task<T> ReadAsJTokenAsync<T>(this HttpContent content, CancellationToken cancellationToken = default(CancellationToken))
            where T : JToken
        {
            cancellationToken.ThrowIfCancellationRequested();

            string str = await content.ReadAsStringAsync().ConfigureAwait(false);

            return (T)JToken.Parse(str);
        }

        public static async Task<string> ReadContentAsStringOrEmptyAsync(this HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.Content == null)
            {
                return string.Empty;
            }

            return await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
