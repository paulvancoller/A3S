/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace za.co.grindrodbank.a3s.Models
{
    [Table("RoleFunction")]
    public class RoleFunctionModel
    {
        [JsonIgnore]
        public Guid RoleId { get; set; }
        public RoleModel Role { get; set; }
        [JsonIgnore]
        public Guid FunctionId { get; set; }
        public FunctionModel Function { get; set; }
    }
}
