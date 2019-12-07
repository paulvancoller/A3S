/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using Microsoft.Extensions.Configuration;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Stores;

namespace za.co.grindrodbank.a3s.tests.Fakes
{
    public class CustomUserStoreFake : CustomUserStore
    {
        public CustomUserStoreFake(A3SContext a3SContext, IConfiguration configuration) : base(a3SContext, null, configuration)
        {
        }
    }
}
