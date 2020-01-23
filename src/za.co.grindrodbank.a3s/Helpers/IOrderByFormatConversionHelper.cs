/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Helpers
{
    public interface IOrderByFormatConversionHelper
    {
        /// <summary>
        /// Converts a list of single term order by strings, where each order by element isrepresented implicitly as "orderByTerm" for ordering "orderByTerm" in an ascending order,
        /// or "OrderByTerm_desc" to indicate that  "orderByTerm" should be ordered in a descending order, into a Key-Value pair list
        /// where the key is the "orderByTerm" and the value is the direction that the term should be ordered (asc or desc).
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> ConvertSingleTermOrderByListToKeyValuePairList(List<string> singleTermOrderByList);

    }
}
