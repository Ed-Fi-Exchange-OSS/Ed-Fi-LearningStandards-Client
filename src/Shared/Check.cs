// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EdFi.Admin.LearningStandards
{
    [DebuggerStepThrough]
    internal static class Check
    {
        /// <summary>
        /// Checks to ensure that the specified <typeparamref name="T"/> value is not null.
        /// </summary>
        /// <typeparam name="T">The value type being checked.</typeparam>
        /// <param name="value">The <typeparamref name="T"/> value to be checked.</param>
        /// <param name="parameterName">The name of the parameter being checked.</param>
        /// <returns>The original T value if not null.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T NotNull<T>(T value, string parameterName)
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        /// <summary>
        /// Checks to ensure that the specified string is not null or empty.
        /// </summary>
        /// <param name="value">The string to be checked.</param>
        /// <param name="parameterName">The name of the parameter being checked.</param>
        /// <returns>The original string value if not null.</returns>
        /// <exception cref="ArgumentNullException">If null</exception>
        /// <exception cref="ArgumentException">If empty</exception>
        public static string NotEmpty(string value, string parameterName)
        {
            Exception e = null;
            if (value is null)
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (value.Trim().Length == 0)
            {
                e = new ArgumentException("The specified parameter was empty", parameterName);
            }

            if (e != null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }

        /// <summary>
        /// Checks to ensure that the specified collection has a count greater than 0, and is not null.
        /// </summary>
        /// <typeparam name="T">The collection entity type</typeparam>
        /// <param name="value">The collection to be checked.</param>
        /// <param name="parameterName">The name of the parameter being checked.</param>
        /// <returns>The original string value if not null.</returns>
        /// <exception cref="ArgumentNullException">If null</exception>
        /// <exception cref="ArgumentException">If empty collection</exception>
        public static ICollection<T> NotEmpty<T>(ICollection<T> value, string parameterName)
        {
            Exception e = null;
            if (value is null)
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (value.Count == 0)
            {
                e = new ArgumentException("The specified collection parameter was empty", parameterName);
            }

            if (e != null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }
    }
}
