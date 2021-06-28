// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using Newtonsoft.Json;

namespace EdFi.Admin.LearningStandards.CLI.Utilities
{
    public class AbstractConverter<TReal, TAbstract> : JsonConverter
        where TReal : TAbstract
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(TAbstract);

        public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
            => serializer.Deserialize<TReal>(reader);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value);
    }
}
