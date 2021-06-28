// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace EdFi.Admin.LearningStandards.Tests.Utilities
{
    public class NUnitLoggerProvider : ILoggerProvider
    {
        private ConcurrentDictionary<string, ILogger> LoggersByCategory => new ConcurrentDictionary<string, ILogger>();

        public ILogger CreateLogger(string categoryName)
            => LoggersByCategory.GetOrAdd(categoryName, s => new NUnitConsoleLogger<object>());

        public void Dispose()
        { }
    }

    public class NUnitConsoleLogger<T> : ILogger<T>, IDisposable
    {
        private readonly LogLevel _logLevel;

        private readonly Action<string> _output;

        public NUnitConsoleLogger()
            : this(LogLevel.Debug)
        {

        }

        public NUnitConsoleLogger(LogLevel logLevel)
        {
            _logLevel = logLevel;
            _output = message =>
            {
                LogLines.Add(message);
                Console.WriteLine(message);
            };
        }

        public List<string> LogLines { get; set; } = new List<string>();

        public void Dispose()
        {
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (logLevel >= _logLevel)
            {
                _output(formatter(state, exception));
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }
    }
}
