/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using NLog;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Helpers
{
    public class ArchiveHelper : IArchiveHelper
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public List<string> ReturnFilesListInTarGz(byte[] bytes, bool flattenFileStructure)
        {
            using var stream = new MemoryStream(bytes);

            List<InMemoryFile> files = ExtractFilesInTarGz(stream, flattenFileStructure);
 
            var returnValues = new List<string>();

            files.ForEach(x =>
            {
                if (flattenFileStructure)
                    returnValues.Add(x.FileName);
                else
                    returnValues.Add(x.FilePath);
            });

            return returnValues;
        }

        public List<InMemoryFile> ExtractFilesFromTarGz(Byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            return ExtractFilesInTarGz(stream, fileNamesOnly: false);
        }

        private List<InMemoryFile> ExtractFilesInTarGz(Stream stream, bool fileNamesOnly)
        {
            // A GZipStream is not seekable, so copy it first to a MemoryStream
            using var gzip = new GZipStream(stream, CompressionMode.Decompress);
            const int chunk = 4096;
            using var memStr = new MemoryStream();
            int read;
            var buffer = new byte[chunk];

            do
            {
                read = gzip.Read(buffer, 0, chunk);
                memStr.Write(buffer, 0, read);
            } while (read == chunk);

            memStr.Seek(0, SeekOrigin.Begin);

            return ExtractFilesInTarFile(memStr, fileNamesOnly);
        }

        private List<InMemoryFile> ExtractFilesInTarFile(Stream stream, bool fileNamesOnly)
        {
            var buffer = new byte[100];
            var outFileList = new List<InMemoryFile>();

            while (true)
            {
                var file = new InMemoryFile();

                // Get file name (100 bytes)
                stream.Read(buffer, 0, 100);
                file.FilePath = Encoding.ASCII.GetString(buffer).Trim('\0');
                if (string.IsNullOrWhiteSpace(file.FilePath))
                    break;      // End of file reached

                // Ignore the file mode, owners and group ID (8 bytes each)
                stream.Seek(24, SeekOrigin.Current);

                // Get file size (12 bytes)
                stream.Read(buffer, 0, 12);
                //string sizeString = Encoding.ASCII.GetString(buffer, 0, 12).Trim();
                //var size = Convert.ToInt64(sizeString.TrimEnd('\0'), 8);
                var size = Convert.ToInt64(Encoding.ASCII.GetString(buffer, 0, 12).Trim(), 8);


                // Ignore remaining header fields
                stream.Seek(376L, SeekOrigin.Current);

                // Read file contents
                var fileContentsBuffer = new byte[size];
                stream.Read(fileContentsBuffer, 0, fileContentsBuffer.Length);

                if (!fileNamesOnly)
                    file.FileContents = fileContentsBuffer;

                if (stream.Position > stream.Length)
                    throw new ArchiveException("There was an error processing the tar ball.");

                var offset = 512 - (stream.Position % 512);
                if (offset == 512)
                    offset = 0;

                stream.Seek(offset, SeekOrigin.Current);

                outFileList.Add(file);
            }

            return outFileList;
        }
    }
}
