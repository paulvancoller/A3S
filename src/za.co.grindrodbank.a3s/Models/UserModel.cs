/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using NpgsqlTypes;

namespace za.co.grindrodbank.a3s.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class UserModel : IdentityUser
    {
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public byte[] Avatar { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedTime { get; set; }

        public Guid? LdapAuthenticationModeId { get; set; }
        public LdapAuthenticationModeModel LdapAuthenticationMode { get; set; }

        public List<UserClaimModel> UserClaims { get; set; }
        public List<UserTokenModel> UserTokens { get; set; }
        public List<UserRoleModel> UserRoles { get; set; }
        public List<UserTeamModel> UserTeams { get; set; }
        public List<TermsOfServiceUserAcceptanceModel> TermsOfServiceAcceptances { get; set; }
        // A User can have many profiles associated with it.
        public List<ProfileModel> Profiles { get; set; }

        public Guid ChangedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public NpgsqlRange<DateTime> SysPeriod { get; set; }
    }
}
