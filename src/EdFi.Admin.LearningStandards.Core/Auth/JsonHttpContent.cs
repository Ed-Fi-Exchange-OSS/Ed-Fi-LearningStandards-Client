// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdFi.Admin.LearningStandards.Core.Auth
{
    /// <summary>
    /// Provides HTTP content based on Json text, or a <see cref="JToken"/> object.
    /// </summary>
    public class JsonHttpContent : StringContent
    {
        /// <summary>
        /// Creates a new instance of the <see cref="JsonHttpContent"/> class.
        /// </summary>
        /// <param name="json">The content used to initialize the <see cref="JsonHttpContent"/></param>
        public JsonHttpContent(string json)
            : base(json, Encoding.UTF8, "application/json")
        {
            Json = JsonConvert.DeserializeObject<JToken>(json);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="JsonHttpContent"/> class.
        /// </summary>
        /// <param name="json">The <see cref="JToken"/> used to initialize the <see cref="JsonHttpContent"/></param>
        public JsonHttpContent(JToken json)
            : base(GetContentString(json), Encoding.UTF8, "application/json")
        {
            Json = json;
        }

        /// <summary>
        /// Gets the JToken object representing the internal content.
        /// </summary>
        public JToken Json { get; }

        private static string GetContentString(JToken token)
        {
            return JsonConvert.SerializeObject(token);
        }
    }
}
