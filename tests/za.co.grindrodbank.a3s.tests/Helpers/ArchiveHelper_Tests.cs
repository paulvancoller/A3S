/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.tests.Helpers
{
    public class ArchiveHelper_Tests : IDisposable
    {
        private readonly string temporaryFolder;
        private readonly byte[] validFileContents;
        private readonly byte[] inValidFileContents;

        public ArchiveHelper_Tests()
        {
            temporaryFolder = $"{ Path.GetTempPath()}{Path.DirectorySeparatorChar}{Guid.NewGuid()}";
            Directory.CreateDirectory(temporaryFolder);

            validFileContents = Convert.FromBase64String("H4sIAF3V110AA+3T0QqCMBQG4K73FHuB9KibQkmvIstmCtPFNo2I3j1t4E1SN6YE+27OBoftjPF7vuGq1pksMs1VV+Xcy7XezAoAYkLwUJOYviqEdm+REAcRjWlCQwKAIUgigA2GeceY1mrDVD/KhbWiY00uheBqoq9vK4oP59iX4LH+iaM83fAd4V4hG7O98upcmh1upKqZ2KMHWntC55e89/yXphaz3vE1/yGM+acBGfJPaOzyv4R0+O0DQqlvF2vP4ziO4yzjCdehEDwADAAA");
            inValidFileContents = Convert.FromBase64String("H4sIAF3V110AA+3T0QqCMBQG4K73FHuB9KibQkmvIstmCtPFNo2I3j1t4E1SN6YE+27OBoftjPF7vuGq1pksMs1VV+Xcy7XezAoAYkLwUJOYviqEdm+REAcRjWlCQwKAIUgigA2GeceY1mrDVD/KhbWiY00uheBqoq9vK4oP59iX4LH+iaM83fAd4V4hG7O98upcmh1upKqZ2KMHWntC55e89/yXphaz3vE1/yGM+acBGfJP");
        }

        public void Dispose()
        {
            if (Directory.Exists(temporaryFolder))
                Directory.Delete(temporaryFolder, true);
        }

        [Fact]
        public void ReturnFilesListInTarGz_NullFileSpecified_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            Exception caughException = null;

            try
            {
                new ArchiveHelper().ReturnFilesListInTarGz(null, false);
            }
            catch (Exception ex)
            {
                caughException = ex;
            }

            // Assert
            Assert.True(caughException is ArgumentNullException, "Null file specified must throw ArgumentNullException.");
        }

        [Fact]
        public void ReturnFilesListInTarGz_WithGzipTarContentSpecified_ExtractsArchive()
        {
            // Arrange
            // Act
            List<string> filesInArchive = new ArchiveHelper().ReturnFilesListInTarGz(validFileContents, true);

            // Assert
            Assert.True(filesInArchive.Count == 2, $"Extracted archive must contain 2 files.");
            Assert.True(string.Compare(Path.GetFileName(filesInArchive[0]), "terms_of_service.css") == 0, $"Returned file 1 must have name of 'terms_of_service.css'.");
            Assert.True(string.Compare(Path.GetFileName(filesInArchive[1]), "terms_of_service.html") == 0, $"Returned file 1 must have name of 'terms_of_service.html'.");
        }

        [Fact]
        public void ExtractFilesFromTarGz_WithGzipTarContentSpecified_ExtractsArchive()
        {
            // Arrange
            // Act
            List<InMemoryFile> filesInArchive = new ArchiveHelper().ExtractFilesFromTarGz(validFileContents);

            // Assert
            Assert.True(filesInArchive.Count == 2, $"Extracted archive must contain 2 files.");
            Assert.True(string.Compare(Path.GetFileName(filesInArchive[0].FileName), "terms_of_service.css") == 0, $"Returned file 1 must have name of 'terms_of_service.css'.");
            Assert.True(string.Compare(Path.GetFileName(filesInArchive[1].FileName), "terms_of_service.html") == 0, $"Returned file 1 must have name of 'terms_of_service.html'.");
        }


        [Fact]
        public void ReturnFilesListInTarGz_WithInvalidArchivespecified_ThrowsArchiveException()
        {
            // Arrange
            string testFolder = $"{temporaryFolder}{Path.DirectorySeparatorChar}{Guid.NewGuid().ToString()}";
            string filePath = $"{testFolder}{Path.DirectorySeparatorChar}test_archive.tar.gz";
            Directory.CreateDirectory(testFolder);

            File.WriteAllBytes(filePath, inValidFileContents);

            // Act
            Exception caughException = null;

            try
            {
                new ArchiveHelper().ReturnFilesListInTarGz(inValidFileContents, true);
            }
            catch (Exception ex)
            {
                caughException = ex;
            }

            // Assert
            Assert.True(caughException is ItemNotProcessableException, "Null file specified must throw ItemNotProcessableException.");
        }
    }
}
