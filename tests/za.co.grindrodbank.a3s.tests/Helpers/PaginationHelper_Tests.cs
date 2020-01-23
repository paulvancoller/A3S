/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace za.co.grindrodbank.a3s.tests.Helpers
{
    public class PaginationHelper_Tests
    {
        IUrlHelper urlHelper;

        public PaginationHelper_Tests()
        {
            urlHelper = CreateUrlHelper();
        }

        private IUrlHelper CreateUrlHelper()
        {
            var urlHelper = Substitute.For<IUrlHelper>();
            var fakeUrl = $"https://www.test-domain.com";
            urlHelper.Link(Arg.Any<string>(), Arg.Any<object>())
                       .Returns(callinfo => $"{fakeUrl}?{ ((dynamic)callinfo.ArgAt<object>(1)).ToString().Replace(" ", "").Replace("{", "").Replace("}", "").Replace(",", "&") }");
            return urlHelper;
        }
    }
}
