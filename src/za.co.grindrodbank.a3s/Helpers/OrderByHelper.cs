/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System.Collections.Generic;
using System.Linq;
using za.co.grindrodbank.a3s.Exceptions;

namespace za.co.grindrodbank.a3s.Helpers
{
    public class OrderByHelper : IOrderByHelper
    {
        public OrderByHelper()
        {
        }

        public List<KeyValuePair<string, string>> ConvertCommaSeparateOrderByStringToKeyValuePairList(string commaSeparateOrderByString)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = new List<KeyValuePair<string, string>>();

            var orderByComponents = commaSeparateOrderByString.Split(',');

            foreach (var orderByTerm in orderByComponents)
            {
                var splitOrderByTermArrray = orderByTerm.Split('_');

                if(splitOrderByTermArrray.Length == 2)
                {
                    // Ensure tha we dont add if a bogus non-direction indicating second term was provided.
                    if (splitOrderByTermArrray[1] == "desc")
                    {
                        orderByKeyValueList.Add(new KeyValuePair<string, string>(splitOrderByTermArrray[0], "desc"));
                        continue;
                    }

                    if (splitOrderByTermArrray[1] == "asc")
                    {
                        orderByKeyValueList.Add(new KeyValuePair<string, string>(splitOrderByTermArrray[0], "asc"));
                        continue;
                    }
                }

                if(splitOrderByTermArrray.Length == 1)
                {
                    orderByKeyValueList.Add(new KeyValuePair<string, string>(splitOrderByTermArrray[0], "asc"));
                    continue;
                }

                throw new InvalidFormatException($"Invalid orderBy parameter supplied. '{orderByTerm}' is not a valid format.");
            }

            return orderByKeyValueList;
        }

        public void ValidateOrderByListOnlyContainsCertainElements(List<KeyValuePair<string, string>> orderByKeyValueList, List<string> desiredOrderByKeys)
        {
            foreach (var orderByListKeyValuePair in orderByKeyValueList)
            {
                if (!desiredOrderByKeys.Contains(orderByListKeyValuePair.Key)){
                    throw new InvalidFormatException($"Invalid orderBy parameter supplied. The '{orderByListKeyValuePair.Key}' component is not valid for this request.");
                }
            }
        }

    }
}
