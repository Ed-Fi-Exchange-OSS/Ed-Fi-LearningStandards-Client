// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace EdFi.Admin.LearningStandards.Tests.Utilities
{
    public class ConsoleStringListWriter : TextWriter
    {
        private readonly TextWriter _out = Console.Out;

        private string _currentLine = string.Empty;

        public List<string> OutputLines { get; set; } = new List<string>();

        public override Encoding Encoding => _out.Encoding;

        public override IFormatProvider FormatProvider => _out.FormatProvider;

        public override string NewLine
        {
            get => _out.NewLine;
            set => _out.NewLine = value;
        }

        public override void Flush()
        {
            _out.Flush();
        }

        public override void Write(char value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(char[] buffer)
        {
            WriteToList(new string(buffer));
            _out.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            WriteToList(new string(buffer.Skip(index).Take(count).ToArray()));
            _out.Write(buffer, index, count);
        }

        public override void Write(bool value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(int value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(uint value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(long value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(ulong value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(float value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(double value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(decimal value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(string value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(object value)
        {
            WriteToList(value);
            _out.Write(value);
        }

        public override void Write(string format, params object[] arg)
        {
            WriteToList(string.Format(format, arg));
            _out.Write(format, arg);
        }

        public override void WriteLine()
        {
            WriteLineToList(string.Empty);
            _out.WriteLine();
        }

        public override void WriteLine(char value)
        {
            WriteLineToList(value.ToString());
            _out.WriteLine(value);
        }

        public override void WriteLine(decimal value)
        {
            WriteLineToList(value.ToString(CultureInfo.InvariantCulture));
            _out.WriteLine(value);
        }

        public override void WriteLine(char[] buffer)
        {
            WriteLineToList(new string(buffer));
            _out.WriteLine(buffer);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            WriteLineToList(new string(buffer.Skip(index).Take(count).ToArray()));
            _out.WriteLine(buffer, index, count);
        }

        public override void WriteLine(bool value)
        {
            WriteLineToList(value.ToString());
            _out.WriteLine(value);
        }

        public override void WriteLine(int value)
        {
            WriteLineToList(value.ToString());
            _out.WriteLine(value);
        }

        public override void WriteLine(uint value)
        {
            OutputLines.Add(value.ToString());
            _out.WriteLine(value);
        }

        public override void WriteLine(long value)
        {
            WriteLineToList(value.ToString());
            _out.WriteLine(value);
        }

        public override void WriteLine(ulong value)
        {
            WriteLineToList(value.ToString());
            _out.WriteLine(value);
        }

        public override void WriteLine(float value)
        {
            WriteLineToList(value.ToString(CultureInfo.InvariantCulture));
            _out.WriteLine(value);
        }

        public override void WriteLine(double value)
        {
            WriteLineToList(value.ToString(CultureInfo.InvariantCulture));
            _out.WriteLine(value);
        }

        public override void WriteLine(string value)
        {
            WriteLineToList(value);
            _out.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            WriteLineToList(value.ToString());
            _out.WriteLine(value);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            WriteLineToList(string.Format(format, arg));
            _out.WriteLine(format, arg);
        }

        private void WriteToList(params object[] values)
        {
            _currentLine += values.Aggregate(string.Concat);
            if (_currentLine.EndsWith(NewLine))
            {
                OutputLines.Add(_currentLine);
                _currentLine = string.Empty;
            }
        }

        private void WriteLineToList(object value)
        {
            WriteToList(value, NewLine);
        }
    }
}
