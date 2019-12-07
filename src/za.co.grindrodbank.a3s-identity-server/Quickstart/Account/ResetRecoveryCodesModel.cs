/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System.Collections.Generic;

namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    public class ResetRecoveryCodesModel
    {
        public string RedirectUrl { get; set; }
        public IEnumerable<string> RecoveryCodes { get; set; }
    }
}
