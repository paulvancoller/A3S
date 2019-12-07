/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace za.co.grindrodbank.a3s.A3SApiResources
{
    public partial class SecurityContract
    {
        [DataMember(Order = 0)]
        [OptionalField]
        [Description("A3S Security Contract\n#\n# This file contains information allowing for configuration and declaration of almost all aspects of A3S.\n# More information at: https://github.com/GrindrodBank/A3S/blob/master/doc/security-contracts.md\n#")]
        public string Name = "A3S Security Contract";

        [DataMember(Order = 0)]
        [OptionalField]
        public string Generated = DateTime.Now.ToString("yyy-mm-dd HH:mm:ss zzz");
    }
}
