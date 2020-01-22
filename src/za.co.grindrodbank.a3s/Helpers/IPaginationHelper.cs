/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Helpers
{
    public interface IPaginationHelper
    {
        /// <summary>
        /// Generates the required X-Pagination response header from a paginated response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paginatedResult">The paginated result instance to generate the header for</param>
        /// <param name="pageRouteName">The name of the route. This route name annotated on the abstract API Controller</param>
        /// <param name="urlHelper"></param>
        /// <param name="response"></param>
        void AddHeaderMetaData<T>(IPaginatedResult<T> paginatedResult, string pageRouteName,
            IUrlHelper urlHelper, HttpResponse response) where T : class;
    }
}
