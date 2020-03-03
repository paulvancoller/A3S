/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Models
{
    public class LatestActiveTransientsForRoleModel
    {
        public List<RoleTransientModel> LatestActiveRoleTransients { get; set; }
        public List<RoleFunctionTransientModel> LatestActiveRoleFunctionTransients { get; set; }
        public List<RoleRoleTransientModel> LatestActiveChildRoleTransients { get; set; }
    }
}
