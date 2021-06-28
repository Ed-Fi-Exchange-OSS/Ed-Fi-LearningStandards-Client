// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Configuration;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests
{
    [TestFixture]
    public class EdFiOdsApiConfigurationHelperTests
    {
        [Test]
        public void Can_resolve_oauth_url()
        {
            //Arrange
            string url = "https://api.domainservesapplication.com";
            string expected = "https://api.domainservesapplication.com/oauth";

            //Act
            var actual = EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, url);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Can_resolve_oauth_url_with_port()
        {
            //Arrange
            string url = "https://api.domainservesapplication.com:12345";
            string expected = "https://api.domainservesapplication.com:12345/oauth";

            //Act
            var actual = EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, url);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Can_resolve_oauth_url_with_slash()
        {
            //Arrange
            string url = "https://api.domainservesapplication.com/";
            string expected = "https://api.domainservesapplication.com/oauth";

            //Act
            var actual = EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, url);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Can_resolve_oauth_url_with_virtual_root()
        {
            //Arrange
            string url = "https://domainhasvirtual.com/applicationFolder";
            string expected = "https://domainhasvirtual.com/applicationFolder/oauth";

            //Act
            var actual = EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, url);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Can_resolve_oauth_url_with_virtual_root_and_port()
        {
            //Arrange
            string url = "https://domainhasvirtual.com:12345/applicationFolder";
            string expected = "https://domainhasvirtual.com:12345/applicationFolder/oauth";

            //Act
            var actual = EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, url);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Can_resolve_oauth_url_with_virtual_root_with_slash()
        {
            //Arrange
            string url = "https://domainhasvirtual.com/applicationFolder/";
            string expected = "https://domainhasvirtual.com/applicationFolder/oauth";

            //Act
            var actual = EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, url);

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        public void Resolve_Authentication_Url_throws_on_empty_url_parameter()
        {
            //Act -> Assert
            Assert.Throws<ArgumentException>(() =>
            {
                EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, "");
            });
        }

        [Test]
        public void Resolve_Authentication_Url_throws_on_improper_url_parameter()
        {
            //Act -> Assert
            //The original .net standard exception is a UriFormatException,
            //however, in portable libraries, the base FormatException is thrown instead.
            Assert.Catch<FormatException>(() =>
            {
                EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, "(test/.c");
            });
        }

        [Test]
        public void Resolve_Authentication_Url_throws_on_null_url_parameter()
        {
            //Act -> Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                EdFiOdsApiConfigurationHelper.ResolveAuthenticationUrl(EdFiOdsApiCompatibilityVersion.v2, null);
            });
        }
    }
}
