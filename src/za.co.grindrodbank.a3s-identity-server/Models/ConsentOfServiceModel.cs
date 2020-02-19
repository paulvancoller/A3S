/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;

namespace za.co.grindrodbank.a3s.Models
{
    public class ConsentOfServiceModel : AuditableModel
    {
        public Guid Id { get; set; }
        public byte[] ConsentFile { get; set; }
    }
}