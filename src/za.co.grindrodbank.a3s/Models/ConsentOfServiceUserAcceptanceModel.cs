/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace za.co.grindrodbank.a3s.Models
{
    public class ConsentOfServiceUserAcceptanceModel
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public NpgsqlRange<DateTime> AcceptanceTime { get; set; }

        public UserModel User { get; set; }

        public List<ConsentOfServiceUserAcceptancePermissionsModel> ConsentOfServiceAcceptancePermissions { get; set; }
    }
}