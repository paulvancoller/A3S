/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
namespace za.co.grindrodbank.a3s.Models
{
    public class ProfileModel : AuditableModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
        // A profile must have one user associated with it.
        public UserModel User { get; set; }
        // A profile must have one sub-realm associated with it.
        public SubRealmModel SubRealm { get; set; }
    }
}
