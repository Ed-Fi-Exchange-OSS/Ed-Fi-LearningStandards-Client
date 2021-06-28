// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.IO;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.CLI;
using EdFi.Admin.LearningStandards.CLI.Internal;
using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Models;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces;
using EdFi.Admin.LearningStandards.Tests.Utilities;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests
{
    [TestFixture]
    public class ChangeSequencePersisterTests
    {
        private IOptions<JsonFileChangeSequencePersisterOptions> _options;

        private string _testFileName = "change-sequence.json";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var mockOptions = new Mock<IOptions<JsonFileChangeSequencePersisterOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new JsonFileChangeSequencePersisterOptions{ FileName = _testFileName });
            _options = mockOptions.Object;
        }

        [Test]
        public async Task Will_return_empty_change_sequence_if_file_is_null()
        {
            //Arrange
            int expected = 0;
            var logger = new NUnitConsoleLogger<JsonFileChangeSequencePersister>();
            IChangeSequencePersister persister = new JsonFileChangeSequencePersister(_options, logger);

            //Act
            var actual = await persister.GetAsync("", "");

            //Assert
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual.Id);
        }

        [Test]
        public void Can_save()
        {
            //Arrange
            var data = new ChangeSequence
                       {
                           Id = 1024,
                           Key = new ChangeSequenceKey("myEdFiKey","myLearningStandardId")
                       };

            var logger = new NUnitConsoleLogger<JsonFileChangeSequencePersister>();
            IChangeSequencePersister persister = new JsonFileChangeSequencePersister(_options, logger);

            //Act - Assert
            Assert.DoesNotThrowAsync(async () =>
            {
                await persister.SaveAsync(data);
            });
            Assert.IsTrue(File.Exists(_testFileName));
        }

        [Test]
        public void Can_not_save_without_key()
        {
            //Arrange
            var data = new ChangeSequence
                       {
                           Id = 1024,
                       };

            var logger = new NUnitConsoleLogger<JsonFileChangeSequencePersister>();
            IChangeSequencePersister persister = new JsonFileChangeSequencePersister(_options, logger);

            //Act - Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await persister.SaveAsync(data);
            });
        }

        [Test]
        public async Task Can_get()
        {
            //Arrange
            var edFiKey = "123";
            var abKey = "abc";
            var rnd = new Random();
            var data = new ChangeSequence
                       { Id = rnd.Next(1000, 2000), Key = new ChangeSequenceKey(edFiKey, abKey) };
            var logger = new NUnitConsoleLogger<JsonFileChangeSequencePersister>();
            IChangeSequencePersister persister = new JsonFileChangeSequencePersister(_options, logger);

            //Act
            await persister.SaveAsync(data);
            var actual = await persister.GetAsync(edFiKey, abKey);

            //Assert
            Assert.AreEqual(data.Id, actual.Id);
            Assert.AreEqual(data.Key.EdFiApiKey, actual.Key.EdFiApiKey);
            Assert.AreEqual(data.Key.LearningStandardCredentialId, actual.Key.LearningStandardCredentialId);
        }

        [Test]
        public async Task Can_get_across_initializations()
        {
            //Arrange
            var edFiKey = "123";
            var abKey = "abc";
            var rnd = new Random();
            var data = new ChangeSequence
                       { Id = rnd.Next(1000, 2000), Key = new ChangeSequenceKey(edFiKey, abKey) };
            var logger = new NUnitConsoleLogger<JsonFileChangeSequencePersister>();
            IChangeSequencePersister firstPersister = new JsonFileChangeSequencePersister(_options, logger);

            //Act
            await firstPersister.SaveAsync(data);

            IChangeSequencePersister secondPersister = new JsonFileChangeSequencePersister(_options, logger);

            var actual = await secondPersister.GetAsync(edFiKey, abKey);

            //Assert
            Assert.AreEqual(data.Id, actual.Id);
            Assert.AreEqual(data.Key.EdFiApiKey, actual.Key.EdFiApiKey);
            Assert.AreEqual(data.Key.LearningStandardCredentialId, actual.Key.LearningStandardCredentialId);
        }

        [Test]
        public async Task Can_overwrite_existing_change_sequence()
        {
            //Arrange
            var edFiKey = "123";
            var abKey = "abc";
            var rnd = new Random();
            var data = new ChangeSequence { Id = rnd.Next(1000, 2000), Key = new ChangeSequenceKey(edFiKey, abKey) };
            var logger = new NUnitConsoleLogger<JsonFileChangeSequencePersister>();
            IChangeSequencePersister persister = new JsonFileChangeSequencePersister(_options, logger);

            //Act
            Console.WriteLine($"Original sequence: {data.Id}");
            await persister.SaveAsync(data);

            var changedData = new ChangeSequence { Id = data.Id + 39, Key = new ChangeSequenceKey(edFiKey, abKey) };
            Console.WriteLine($"New sequence: {changedData.Id}");
            await persister.SaveAsync(changedData);

            var actual = await persister.GetAsync(edFiKey, abKey);

            //Assert
            Assert.AreEqual(changedData.Id, actual.Id);
            Assert.AreEqual(changedData.Key, actual.Key);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testFileName))
            {
                File.Delete(_testFileName);
            }
        }
    }
}
