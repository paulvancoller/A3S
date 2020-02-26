/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;

namespace za.co.grindrodbank.a3s.Models
{
    public class ConsentOfServiceUserAcceptancePermissionsModel
    {
        public Guid Id { get; set; }

        public Guid PermissionId { get; set; }

        public PermissionModel Permission { get; set; }

        public Guid ConsentOfServiceUserAcceptanceId { get; set; }

        public ConsentOfServiceUserAcceptanceModel ConsentAcceptance { get; set; }
    }
}