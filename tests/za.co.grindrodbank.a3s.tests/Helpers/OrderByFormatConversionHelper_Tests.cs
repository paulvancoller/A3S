using System;
using System.Collections.Generic;
using Xunit;
using za.co.grindrodbank.a3s.Helpers;

namespace za.co.grindrodbank.a3s.tests.Helpers
{
    public class OrderByFormatConversionHelper_Tests
    {
        public OrderByFormatConversionHelper_Tests()
        {
        }

        [Fact]
        public void GetKeyValueListWithOrderByTermAndOrderDirection_GivenSingleStringOrderByList()
        {
            List<string> singleTermOrderByList = new List<string>
            {
                "testTerm1",
                "testTerm2_desc",
                "testTerm3_asc"
            };

            var orderByFormatConversionHelper = new OrderByFormatConversionHelper();

            var convertedKeyValuePairList = orderByFormatConversionHelper.ConvertSingleTermOrderByListToKeyValuePairList(singleTermOrderByList);

            Assert.True(convertedKeyValuePairList[0].Key == "testTerm1", $"Expected first converted orderBy term key to be 'testTerm1' but actual value is {convertedKeyValuePairList[0].Key}");
            Assert.True(convertedKeyValuePairList[0].Value == "asc", $"Expected first covnerted orderBy term value to be 'asc' but actual value is {convertedKeyValuePairList[0].Key}");

            Assert.True(convertedKeyValuePairList[1].Key == "testTerm2", $"Expected second converted orderBy term key to be 'testTerm2' but actual value is {convertedKeyValuePairList[1].Key}");
            Assert.True(convertedKeyValuePairList[1].Value == "desc", $"Expected second covnerted orderBy term value to be 'desc' but actual value is {convertedKeyValuePairList[1].Key}");

            Assert.True(convertedKeyValuePairList[2].Key == "testTerm3", $"Expected third converted orderBy term key to be 'testTerm3' but actual value is {convertedKeyValuePairList[2].Key}");
            Assert.True(convertedKeyValuePairList[2].Value == "asc", $"Expected third covnerted orderBy term value to be 'asc' but actual value is {convertedKeyValuePairList[2].Key}");
        }

        [Fact]
        public void GetKeyValueListWithOrderByTermAndOrderDirectionWithoutErroneousInput_GivenErroneousSingleStringOrderByList()
        {
            List<string> singleTermOrderByList = new List<string>
            {
                "testTerm1_desc_test",
                "testTerm2_rubbish"
            };

            var orderByFormatConversionHelper = new OrderByFormatConversionHelper();

            var convertedKeyValuePairList = orderByFormatConversionHelper.ConvertSingleTermOrderByListToKeyValuePairList(singleTermOrderByList);

            Assert.True(convertedKeyValuePairList.Count == 0, $"Expected all erroneous terms to be filtered out of conversion, but '{convertedKeyValuePairList.Count}' terms were included.");
        }
    }
}
