/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class PaginatedResult<T> : PaginatedResultBase where T : class
    {
        public IList<T> Results { get; set; }

        public PaginatedResult()
        {
            Results = new List<T>();
        }
    }
}
