// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Services;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests
{
    [TestFixture]
    public class EdFiBulkJsonPersisterHelperTests
    {
        private const string _v2BaseUrl = "https://api.ed-fi.org/v2.5.0/api";

        private const string _v3BaseUrl = "https://api.ed-fi.org/v3.0/api";

        private const string _resource = "classPeriods";

        [Test]
        public void Can_resolve_v2_address()
        {
            //Arrange
            string schema = string.Empty;
            var version = EdFiOdsApiCompatibilityVersion.v2;
            int? schoolYear = 2018;

            string expected = $"{_v2BaseUrl}/api/v2.0/{schoolYear}/{_resource}";

            //Act
            var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(_v2BaseUrl, schema, _resource, version, schoolYear);

            //Assert
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Version_2_throws_on_null_school_year()
        {
            //Arrange
            var version = EdFiOdsApiCompatibilityVersion.v2;

            //Act -> Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(_v2BaseUrl, string.Empty, _resource, version, null);
            });
        }

        [Test]
        public void Version_2_throws_on_improper_base_url()
        {
            //Arrange
            var version = EdFiOdsApiCompatibilityVersion.v2;

            //Act -> Assert
            //The original .net standard exception is a UriFormatException,
            //however, in portable libraries, the base FormatException is thrown instead.
            Assert.Catch<FormatException>(() =>
            {
                var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl("/api.ed-fi.org", string.Empty, _resource, version, 2018);
            });
        }

        [Test]
        public void Version_2_throws_on_empty_resource()
        {
            //Arrange
            string resource = string.Empty;
            var version = EdFiOdsApiCompatibilityVersion.v2;

            //Act -> Assert
            Assert.Throws<ArgumentException>(() =>
            {
                var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(_v2BaseUrl, string.Empty, resource, version, 2018);
            });
        }

        [Test]
        public void Can_resolve_v3_address()
        {
            //Arrange
            string schema = "ed-fi";
            var version = EdFiOdsApiCompatibilityVersion.v3;

            string expected = $"{_v3BaseUrl}/data/v3/ed-fi/{_resource}";

            //Act
            var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(_v3BaseUrl, schema, _resource, version, null);

            //Assert
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Version_3_includes_year_when_provided()
        {
            //Arrange
            string schema = "ed-fi";
            int? year = 2018;
            var version = EdFiOdsApiCompatibilityVersion.v3;

            string expected = $"{_v3BaseUrl}/data/v3/{year}/ed-fi/{_resource}";

            //Act
            var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(_v3BaseUrl, schema, _resource, version, year);

            //Assert
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Version_3_throws_on_empty_schema()
        {
            //Arrange
            string schema = string.Empty;
            var version = EdFiOdsApiCompatibilityVersion.v3;

            //Act -> Assert

            Assert.Throws<ArgumentException>(() =>
            {
                var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(_v3BaseUrl, schema, _resource, version, null);
            });
        }

        [Test]
        public void Version_3_throws_on_improper_base_url()
        {
            //Arrange
            string schema = "ed-fi";
            var version = EdFiOdsApiCompatibilityVersion.v3;

            //Act -> Assert
            //The original .net standard exception is a UriFormatException,
            //however, in portable libraries, the base FormatException is thrown instead.
            Assert.Catch<FormatException>(() =>
            {
                var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl("/api.ed-fi.org", schema, _resource, version, null);
            });
        }

        [Test]
        public void Version_3_throws_on_empty_resource()
        {
            //Arrange
            string resource = string.Empty;
            var version = EdFiOdsApiCompatibilityVersion.v2;

            //Act -> Assert
            Assert.Throws<ArgumentException>(() =>
            {
                var actual = EdFiBulkJsonPersisterHelper.ResolveOdsApiResourceUrl(_v3BaseUrl, string.Empty, resource, version, null);
            });
        }
    }
}
