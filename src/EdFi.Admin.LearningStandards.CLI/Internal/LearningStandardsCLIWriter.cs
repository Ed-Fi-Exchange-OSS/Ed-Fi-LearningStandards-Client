// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EdFi.Admin.LearningStandards.CLI
{
    public class LearningStandardsCLIWriter
    {
        private readonly bool _verbose;

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public LearningStandardsCLIWriter(bool verbose, JsonSerializerSettings jsonSerializerSettings = null)
        {
            _verbose = verbose;
            _jsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings
            {
                 Formatting = Formatting.Indented,
                 ContractResolver = new DefaultContractResolver
                 {
                     NamingStrategy = new CamelCaseNamingStrategy()
                 }
            };
        }

        public LearningStandardsCLIWriter Info(string message)
        {
            Info(message, null);
            return this;
        }

        public LearningStandardsCLIWriter Info(string message, string verboseContent)
        {
            WriteLineInternal(message);
            if (verboseContent != null)
            {
                Verbose(verboseContent);
            }

            return this;
        }

        public LearningStandardsCLIWriter Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ResetColor();
            WriteLineInternal(message);
            return this;
        }

        public LearningStandardsCLIWriter Verbose(string message)
        {
            if (_verbose && !string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Verbose: ");
                Console.ResetColor();
                WriteLineInternal(message);
            }

            return this;
        }

        public LearningStandardsCLIWriter Json(object value)
        {
            WriteRaw(JsonConvert.SerializeObject(value, _jsonSerializerSettings));
            return this;
        }

        internal LearningStandardsCLIWriterBuilder BuildInfo()
        {
            return new LearningStandardsCLIWriterBuilder(Info);
        }

        internal LearningStandardsCLIWriterBuilder BuildError()
        {
            return new LearningStandardsCLIWriterBuilder(Error);
        }

        internal LearningStandardsCLIWriterBuilder BuildVerbose()
        {
            return new LearningStandardsCLIWriterBuilder(Verbose);
        }

        private void WriteLineInternal(string message)
        {
            Console.WriteLine(message.TrimStart('\r', '\n'));
        }

        private void WriteRaw(string content)
        {
            Console.Write(content);
        }
    }

    internal class LearningStandardsCLIWriterBuilder
    {
        private readonly Func<string, LearningStandardsCLIWriter> _output;

        private readonly StringBuilder _builder = new StringBuilder();

        public LearningStandardsCLIWriterBuilder(Func<string, LearningStandardsCLIWriter> output)
        {
            _output = output;
        }

        public LearningStandardsCLIWriterBuilder AppendLine(string value)
        {
            _builder.AppendLine(value);
            return this;
        }

        public LearningStandardsCLIWriterBuilder AppendFormatLine(string format, params object[] args)
        {
            _builder.AppendFormat(format, args);
            _builder.Append(Environment.NewLine);
            return this;
        }

        public LearningStandardsCLIWriter Write()
        {
            return _output?.Invoke(_builder.ToString());
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
