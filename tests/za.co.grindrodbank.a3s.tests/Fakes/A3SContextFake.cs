/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using Microsoft.EntityFrameworkCore;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.tests.Fakes
{
    public class A3SContextFake : A3SContext
    {
        public A3SContextFake(DbContextOptions<A3SContext> options) : base(options)
        {

        }
    }
}
