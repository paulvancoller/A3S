/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.IO;

namespace za.co.grindrodbank.a3s.Models
{
    public class InMemoryFile
    {
        public string FilePath { get; set; }
        public byte[] FileContents { get; set; }

        public string FileName
        {
            get
            {
                return Path.GetFileName(FilePath);
            }
        }
        public long FileSize
        {
            get
            {
                return FileContents.Length;
            }
        }
    }
}
