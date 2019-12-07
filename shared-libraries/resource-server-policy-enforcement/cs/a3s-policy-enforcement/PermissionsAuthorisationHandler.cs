/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace za.co.grindrodbank.a3s.AuthorisationPolicies
{
    public class PermissionsAuthorisationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Bail out if the permission user claim isn't present
            if (!context.User.HasClaim(c => c.Type == "permission"))
            {
                return Task.CompletedTask;
            }

            // The permission user claim can either be a string, or a list, depending if there is a single permission or whether there are many.
            // Always try get this as a List of claims for safety.
            var permissionClaims = context.User.Claims.Where(c => c.Type == "permission").ToList();

            // Ensure that there is actuall something within the claim map. if not, return without success.
            if (permissionClaims == null || permissionClaims.Count == 0)
            {
                return Task.CompletedTask;
            }

            // Check if the assessed permission exists within the permissionClaim list extracted from the token.
            foreach (var permissionClaim in permissionClaims)
            {
                if (permissionClaim.Value == requirement.PermissionName)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
