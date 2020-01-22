using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Helpers
{
    public class PaginationHelper : IPaginationHelper
    {
        public void AddHeaderMetaData(IPaginatedResult paginatedResult, string pageRouteName
            , IUrlHelper urlHelper, HttpResponse response)
        {
            var previousPageLink = paginatedResult.CurrentPage > 0
                ? CreatePagedLinkRelToCurrentPage(paginatedResult, urlHelper, pageRouteName, -1)
                : null;


            var nextPageLink = HasNextPage(paginatedResult)
                ? CreatePagedLinkRelToCurrentPage(paginatedResult, urlHelper, pageRouteName, 1)
                : null;

            var first = CreatePageLink(pageSize: paginatedResult.PageSize,
                                       pageNumber: 0,  // Zero based page numbering
                                       pageRouteName: pageRouteName,
                                       urlHelper: urlHelper);

            var last = CreatePageLink(pageSize: paginatedResult.PageSize,
                                       pageNumber: GetTotalPages(paginatedResult),
                                       pageRouteName: pageRouteName,
                                       urlHelper: urlHelper);

            var paginationMetadata = new
            {
                total = paginatedResult.RowCount,
                size = paginatedResult.PageSize,
                count = Enumerable.Count(paginatedResult.Results),
                current = paginatedResult.CurrentPage,
                prev = previousPageLink,
                next = nextPageLink,
                first,
                last
            };

            response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
        }

        private int GetTotalPages(IPaginatedResult paginatedResult)
        {
            return Convert.ToInt32(Math.Ceiling(((decimal)paginatedResult.RowCount / (decimal)paginatedResult.PageSize)))
                - 1; // Zero based page numbering
        }

        private bool HasNextPage(IPaginatedResult paginatedResult)
        {
            return paginatedResult.RowCount >
                paginatedResult.CurrentPage * paginatedResult.PageSize;
        }


        private string CreatePagedLinkRelToCurrentPage(IPaginatedResult paginatedResult, IUrlHelper urlHelper,
            string pageRouteName, int pagesFromCurrentPage)
        {
            return CreatePageLink(pageSize: paginatedResult.PageSize,
                                   pageNumber: paginatedResult.CurrentPage + pagesFromCurrentPage,
                                   pageRouteName: pageRouteName,
                                   urlHelper: urlHelper);
        }

        private string CreatePageLink(int pageSize, int pageNumber, string pageRouteName, IUrlHelper urlHelper)
        {
            return urlHelper.Link(pageRouteName,
                      new
                      {
                          pageNumber,
                          pageSize
                      });
        }
    }
}
