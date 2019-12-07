/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.IO;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Helpers
{
    public interface IArchiveHelper
    {
        /// <summary>
        /// Fetches a list of files inside a .tar.gz file.
        /// </summary>
        /// <param name="bytes">Byte array contianing the .tar.gz file contents.</param>
        /// <param name="flattenFileStructure">Return only the file names with no directory information.</param>
        /// <returns></returns>
        List<string> ReturnFilesListInTarGz(byte[] bytes, bool flattenFileStructure);

        /// <summary>
        /// Extract files from a .tar.gz file.
        /// </summary>
        /// <param name="bytes">Byte array contianing the .tar.gz file contents.</param>
        /// <param name="filesFilter">A list of files to filter the return extract list by. If empty, extract all.</param>
        /// <returns></returns>
        List<InMemoryFile> ExtractFilesFromTarGz(Byte[] bytes);
    }
}
