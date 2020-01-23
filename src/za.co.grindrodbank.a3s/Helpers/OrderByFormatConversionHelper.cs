using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Helpers
{
    public class OrderByFormatConversionHelper : IOrderByFormatConversionHelper
    {
        public OrderByFormatConversionHelper()
        {
        }

        public List<KeyValuePair<string, string>> ConvertSingleTermOrderByListToKeyValuePairList(List<string> singleTermOrderByList)
        {
            List<KeyValuePair<string, string>> orderByKeyValueList = new List<KeyValuePair<string, string>>();

            foreach (var orderByTerm in singleTermOrderByList)
            {
                var splitOrderByTermArrray = orderByTerm.Split('_');

                if(splitOrderByTermArrray.Length == 2)
                {
                    // Ensure tha we dont add if a bogus non-direction indicating second term was provided.
                    if (splitOrderByTermArrray[1] == "desc")
                    {
                        orderByKeyValueList.Add(new KeyValuePair<string, string>(splitOrderByTermArrray[0], "desc"));
                    }

                    if (splitOrderByTermArrray[1] == "asc")
                    {
                        orderByKeyValueList.Add(new KeyValuePair<string, string>(splitOrderByTermArrray[0], "asc"));
                    }
                }

                if(splitOrderByTermArrray.Length == 1)
                {
                    orderByKeyValueList.Add(new KeyValuePair<string, string>(splitOrderByTermArrray[0], "asc"));
                }
            }

            return orderByKeyValueList;
        }
    }
}
