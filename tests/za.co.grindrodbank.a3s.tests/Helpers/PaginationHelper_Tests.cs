/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Helpers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.tests.Helpers
{
    public class PaginationHelper_Tests
    {
        public PaginationHelper_Tests()
        {

        }

        private IUrlHelper CreateMockUrlHelper()
        {
            var urlHelper = Substitute.For<IUrlHelper>();
            var fakeUrl = $"https://www.test-domain.com";

            urlHelper.Link(Arg.Any<string>(), Arg.Any<object>())
           .Returns(callinfo => $"{fakeUrl}?page={ ((dynamic)callinfo.ArgAt<object>(1)).page }&size={ ((dynamic)callinfo.ArgAt<object>(1)).size }&filterName=testFilterName&orderBy=test1_desc,test2");
            return urlHelper;
        }

        private IPaginatedResult<ApplicationModel> CreateMockPaginatedResult(int pageSize, int pageCount, int rowCount, int currentPage)
        {
            PaginatedResult<ApplicationModel> paginatedResult = new PaginatedResult<ApplicationModel>
            {
                PageSize = pageSize,
                PageCount = pageCount,
                RowCount = rowCount,
                CurrentPage = currentPage,
                Results = CreateMockApplicationModelResultSet()
            };

            return paginatedResult;
        }

        private List<ApplicationModel> CreateMockApplicationModelResultSet()
        {
            List<string> applicationValues = new List<string> { "One", "Two" , "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };

            List<ApplicationModel> applicationModels = new List<ApplicationModel>();

            foreach(var applicationValue in applicationValues)
            {
                applicationModels.Add(new ApplicationModel
                {
                    Name = $"Application{applicationValue}"
                });
            }

            return applicationModels;
        }

        private HttpResponse CreateMockHttpResponse(IHeaderDictionary headers = null)
        {
            var httpResponse = Substitute.For<HttpResponse>();
            httpResponse.Headers.Returns(headers ?? Substitute.For<IHeaderDictionary>());

            return httpResponse;
        }

        private List<KeyValuePair<string, string>> CreateMockCurrentFilters()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("filterName", "testFilterName")
            };
        }

        private string CreateMockOrderByQueryParams()
        {
            return "test1_desc,test2";
        }

        [Fact]
        public void ObtainTenPageMetaData_GivenPageSizeOfOneOnFirstPage()
        {
            PaginationHelper paginationHelper = new PaginationHelper();

            PaginationHeaderResponse paginationHeaderResponse = paginationHelper.AddPaginationHeaderMetaDataToResponse(
                CreateMockPaginatedResult(1, 1, 10, 1),
                CreateMockCurrentFilters(),
                CreateMockOrderByQueryParams(),
                "ApplicationList",
                CreateMockUrlHelper(),
                CreateMockHttpResponse()
                );

            Assert.True(paginationHeaderResponse.Current == 1, $"Expecting current page to be 1, but actual current value is {paginationHeaderResponse.Current}");
            Assert.True(paginationHeaderResponse.Count == 10, $"Expecting result count to be 10, but actual current value is {paginationHeaderResponse.Count}");
            Assert.True(paginationHeaderResponse.Size == 1, $"Expecting result page size to be 1, but actual current value is {paginationHeaderResponse.Size}");
            Assert.True(paginationHeaderResponse.Total == 10, $"Expecting result total to be 10, but actual current value is {paginationHeaderResponse.Total}");
            string expectedNextUrl = "https://www.test-domain.com?page=2&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Next == expectedNextUrl, $"Expecting next URL to be '{expectedNextUrl}', but actual value is '{paginationHeaderResponse.Next}'");
            Assert.True(paginationHeaderResponse.Prev == null, $"Expecting null Prev URL, but obtained the following {paginationHeaderResponse.Prev}");
            string expectedFirstUrl = "https://www.test-domain.com?page=1&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.First == expectedFirstUrl, $"Expected a first page URL of '{expectedFirstUrl}', but obtained the following: '{paginationHeaderResponse.First}'");
            string expectedLastUrl = "https://www.test-domain.com?page=10&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Last == expectedLastUrl, $"Expected a last page URL of '{expectedLastUrl}', but obtained the following: '{paginationHeaderResponse.Last}'");
        }

        [Fact]
        public void ObtainTenPageMetaData_GivenPageSizeOfOneOnSecondPage()
        {
            PaginationHelper paginationHelper = new PaginationHelper();

            PaginationHeaderResponse paginationHeaderResponse = paginationHelper.AddPaginationHeaderMetaDataToResponse(
                CreateMockPaginatedResult(1, 1, 10, 2),
                CreateMockCurrentFilters(),
                CreateMockOrderByQueryParams(),
                "ApplicationList",
                CreateMockUrlHelper(),
                CreateMockHttpResponse()
                );

            Assert.True(paginationHeaderResponse.Current == 2, $"Expecting current page to be 2, but actual current value is {paginationHeaderResponse.Current}");
            Assert.True(paginationHeaderResponse.Count == 10, $"Expecting result count to be 10, but actual current value is {paginationHeaderResponse.Count}");
            Assert.True(paginationHeaderResponse.Size == 1, $"Expecting result page size to be 1, but actual current value is {paginationHeaderResponse.Size}");
            Assert.True(paginationHeaderResponse.Total == 10, $"Expecting result total to be 10, but actual current value is {paginationHeaderResponse.Total}");
            string expectedNextUrl = "https://www.test-domain.com?page=3&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Next == expectedNextUrl, $"Expecting next URL to be '{expectedNextUrl}', but actual value is '{paginationHeaderResponse.Next}'");
            string expectedPrevPage = "https://www.test-domain.com?page=1&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Prev == expectedPrevPage, $"Expecting Prev URL to be '{expectedPrevPage}', but obtained the following {paginationHeaderResponse.Prev}");
            string expectedFirstUrl = "https://www.test-domain.com?page=1&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.First == expectedFirstUrl, $"Expected a first page URL of '{expectedFirstUrl}', but obtained the following: '{paginationHeaderResponse.First}'");
            string expectedLastUrl = "https://www.test-domain.com?page=10&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Last == expectedLastUrl, $"Expected a last page URL of '{expectedLastUrl}', but obtained the following: '{paginationHeaderResponse.Last}'");
        }

        [Fact]
        public void ObtainTenPageMetaData_GivenPageSizeOfOneOnTenthPage()
        {
            PaginationHelper paginationHelper = new PaginationHelper();

            PaginationHeaderResponse paginationHeaderResponse = paginationHelper.AddPaginationHeaderMetaDataToResponse(
                CreateMockPaginatedResult(1, 1, 10, 10),
                CreateMockCurrentFilters(),
                CreateMockOrderByQueryParams(),
                "ApplicationList",
                CreateMockUrlHelper(),
                CreateMockHttpResponse()
                );

            Assert.True(paginationHeaderResponse.Current == 10, $"Expecting current page to be 10, but actual current value is {paginationHeaderResponse.Current}");
            Assert.True(paginationHeaderResponse.Count == 10, $"Expecting result count to be 10, but actual current value is {paginationHeaderResponse.Count}");
            Assert.True(paginationHeaderResponse.Size == 1, $"Expecting result page size to be 1, but actual current value is {paginationHeaderResponse.Size}");
            Assert.True(paginationHeaderResponse.Total == 10, $"Expecting result total to be 10, but actual current value is {paginationHeaderResponse.Total}");
            Assert.True(paginationHeaderResponse.Next == null, $"Expecting next URL to be null, but actual value is '{paginationHeaderResponse.Next}'");
            string expectedPrevPage = "https://www.test-domain.com?page=9&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Prev == expectedPrevPage, $"Expecting Prev URL to be '{expectedPrevPage}', but obtained the following {paginationHeaderResponse.Prev}");
            string expectedFirstUrl = "https://www.test-domain.com?page=1&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.First == expectedFirstUrl, $"Expected a first page URL of '{expectedFirstUrl}', but obtained the following: '{paginationHeaderResponse.First}'");
            string expectedLastUrl = "https://www.test-domain.com?page=10&size=1&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Last == expectedLastUrl, $"Expected a last page URL of '{expectedLastUrl}', but obtained the following: '{paginationHeaderResponse.Last}'");
        }

        [Fact]
        public void ObtainFivePageMetaData_GivenPageSizeOfTwoOnFirstPage()
        {
            PaginationHelper paginationHelper = new PaginationHelper();

            PaginationHeaderResponse paginationHeaderResponse = paginationHelper.AddPaginationHeaderMetaDataToResponse(
                CreateMockPaginatedResult(2, 5, 10, 1),
                CreateMockCurrentFilters(),
                CreateMockOrderByQueryParams(),
                "ApplicationList",
                CreateMockUrlHelper(),
                CreateMockHttpResponse()
                );

            Assert.True(paginationHeaderResponse.Current == 1, $"Expecting current page to be 1, but actual current value is {paginationHeaderResponse.Current}");
            Assert.True(paginationHeaderResponse.Count == 10, $"Expecting result count to be 10, but actual current value is {paginationHeaderResponse.Count}");
            Assert.True(paginationHeaderResponse.Size == 2, $"Expecting result page size to be 2, but actual current value is {paginationHeaderResponse.Size}");
            Assert.True(paginationHeaderResponse.Total == 5, $"Expecting result total pages to be 5, but actual current value is {paginationHeaderResponse.Total}");
            string expectedNextUrl = "https://www.test-domain.com?page=2&size=2&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Next == expectedNextUrl, $"Expecting next URL to be '{expectedNextUrl}', but actual value is '{paginationHeaderResponse.Next}'");
            Assert.True(paginationHeaderResponse.Prev == null, $"Expecting null Prev URL, but obtained the following {paginationHeaderResponse.Prev}");
            string expectedFirstUrl = "https://www.test-domain.com?page=1&size=2&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.First == expectedFirstUrl, $"Expected a first page URL of '{expectedFirstUrl}', but obtained the following: '{paginationHeaderResponse.First}'");
            string expectedLastUrl = "https://www.test-domain.com?page=5&size=2&filterName=testFilterName&orderBy=test1_desc,test2";
            Assert.True(paginationHeaderResponse.Last == expectedLastUrl, $"Expected a last page URL of '{expectedLastUrl}', but obtained the following: '{paginationHeaderResponse.Last}'");
        }
    }
}
