/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;

namespace za.co.grindrodbank.a3s.Helpers
{
    public static class ClaimsHelper
    {
        public static T GetScalarClaimValue<T>(ClaimsPrincipal claimsPrincipal, string claimType, T defaultValue)
        {
            Claim claim = null;

            if (claimsPrincipal != null)
                claim = claimsPrincipal.FindFirst(claimType);

            if (claim != null)
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(claim.Value);

            return defaultValue;
        }

        public static List<string> GetDataPolicies(ClaimsPrincipal claimsPrincipal)
        {
            List<string> dataPolicyList = new List<string>();

            if (claimsPrincipal != null)
            {
                var claims = claimsPrincipal.FindAll("dataPolicy");

                foreach (var claim in claims)
                {
                    dataPolicyList.Add(claim.Value);
                }
            }

            return dataPolicyList;
        }
    }
}
