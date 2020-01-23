/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using Xunit;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Helpers;

namespace za.co.grindrodbank.a3s.tests.Helpers
{
    public class OrderByHelper_Tests
    {
        public OrderByHelper_Tests()
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

            var orderByHelper = new OrderByHelper();

            var convertedKeyValuePairList = orderByHelper.ConvertSingleTermOrderByListToKeyValuePairList(singleTermOrderByList);

            Assert.True(convertedKeyValuePairList[0].Key == "testTerm1", $"Expected first converted orderBy term key to be 'testTerm1' but actual value is {convertedKeyValuePairList[0].Key}");
            Assert.True(convertedKeyValuePairList[0].Value == "asc", $"Expected first covnerted orderBy term value to be 'asc' but actual value is {convertedKeyValuePairList[0].Key}");

            Assert.True(convertedKeyValuePairList[1].Key == "testTerm2", $"Expected second converted orderBy term key to be 'testTerm2' but actual value is {convertedKeyValuePairList[1].Key}");
            Assert.True(convertedKeyValuePairList[1].Value == "desc", $"Expected second covnerted orderBy term value to be 'desc' but actual value is {convertedKeyValuePairList[1].Key}");

            Assert.True(convertedKeyValuePairList[2].Key == "testTerm3", $"Expected third converted orderBy term key to be 'testTerm3' but actual value is {convertedKeyValuePairList[2].Key}");
            Assert.True(convertedKeyValuePairList[2].Value == "asc", $"Expected third covnerted orderBy term value to be 'asc' but actual value is {convertedKeyValuePairList[2].Key}");
        }

        [Fact]
        public void GetKeyValueListWithOrderByTerm_ExpectInvalidaFormatException_GivenErroneousMultiSeparatorSingleStringOrderByList()
        {
            List<string> singleTermOrderByList = new List<string>
            {
                "testTerm1_desc_test",
            };

            var orderByHelper = new OrderByHelper();

            Assert.Throws<InvalidFormatException>(() => orderByHelper.ConvertSingleTermOrderByListToKeyValuePairList(singleTermOrderByList));
        }

        [Fact]
        public void GetKeyValueListWithOrderByTerm_ExpectInvalidaFormatException_GivenErroneousDirectionalSingleStringOrderByList()
        {
            List<string> singleTermOrderByList = new List<string>
            {
                "testTerm1_rubbish",
            };

            var orderByHelper = new OrderByHelper();

            Assert.Throws<InvalidFormatException>(() => orderByHelper.ConvertSingleTermOrderByListToKeyValuePairList(singleTermOrderByList));
        }

        [Fact]
        public void ValidateKeyValueListWithOrderByTermIsTrue_GivenValidDesiredOrderByElements()
        {
            List<string> singleTermOrderByList = new List<string>
            {
                "testTerm1",
                "testTerm2_desc",
                "testTerm3_asc"
            };

            List<string> desiredOrderByTerms = new List<string>
            {
                "testTerm1",
                "testTerm2",
                "testTerm3"
            };

            var orderByHelper = new OrderByHelper();
            var convertedList = orderByHelper.ConvertSingleTermOrderByListToKeyValuePairList(singleTermOrderByList);

            try
            {
                orderByHelper.ValidateOrderByListOnlyContainsCertainElements(convertedList, desiredOrderByTerms);
            }
            catch (InvalidFormatException invalidFormatException)
            {
                Assert.True(false, $"Expected no Exception but an invalid formate exception was thrown with message '{invalidFormatException.Message}'");
            }

            Assert.True(true);
        }

        [Fact]
        public void ValidateKeyValueListWithOrderByTermThrowsException_GivenInValidDesiredOrderByElements()
        {
            List<string> singleTermOrderByList = new List<string>
            {
                "testTerm1",
                "testTerm2_desc",
                "testTerm5_asc"
            };

            List<string> desiredOrderByTerms = new List<string>
            {
                "testTerm1",
                "testTerm2",
                "testTerm3"
            };

            var orderByHelper = new OrderByHelper();
            var convertedList = orderByHelper.ConvertSingleTermOrderByListToKeyValuePairList(singleTermOrderByList);

            Assert.Throws<InvalidFormatException>(() => orderByHelper.ValidateOrderByListOnlyContainsCertainElements(convertedList, desiredOrderByTerms));
        }
    }
}
