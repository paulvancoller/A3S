/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using Xunit;
using za.co.grindrodbank.a3sidentityserver.Quickstart.UI;

namespace za.co.grindrodbank.a3sidentityserver.tests.Quickstart.Home
{
    public class ErrorViewModel_Tests
    {
        [Fact]
        public void ErrorViewModel_Instantiate_NoError()
        {
            try
            {
                ErrorViewModel errorViewModel = new ErrorViewModel();
                Assert.True(true, "Instantiating ErrorViewModel must not error.");
            }
            catch
            {
                Assert.True(false, "Instantiating ErrorViewModel must not error.");
            }
        }
    }
}
