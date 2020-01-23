/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Helpers
{
    public class PaginationHelper : IPaginationHelper
    {
        public void AddHeaderMetaData<T>(IPaginatedResult<T> paginatedResult, List<KeyValuePair<string, string>> filters, List<string> orderBy, string pageRouteName,
            IUrlHelper urlHelper, HttpResponse response) where T : class
        {
            var previousPageLink = paginatedResult.CurrentPage > 1
                ? CreatePagedLinkRelToCurrentPage(paginatedResult, filters, orderBy, urlHelper, pageRouteName, -1)
                : null;


            var nextPageLink = HasNextPage(paginatedResult)
                ? CreatePagedLinkRelToCurrentPage(paginatedResult, filters, orderBy, urlHelper, pageRouteName, 1)
                : null;

            var first = CreatePageLink(pageSize: paginatedResult.PageSize,
                                       pageNumber: 1, 
                                       pageRouteName: pageRouteName,
                                       filters: filters,
                                       orderBy: orderBy,
                                       urlHelper: urlHelper);

            var last = CreatePageLink(pageSize: paginatedResult.PageSize,
                                       pageNumber: GetTotalPages(paginatedResult),
                                       pageRouteName: pageRouteName,
                                       filters: filters,
                                       orderBy: orderBy,
                                       urlHelper: urlHelper);

            var paginationMetadata = new
            {
                total = GetTotalPages(paginatedResult),
                size = paginatedResult.PageSize,
                count = paginatedResult.RowCount,
                current = paginatedResult.CurrentPage,
                prev = previousPageLink,
                next = nextPageLink,
                first,
                last
            };

            response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
        }

        private int GetTotalPages<T>(IPaginatedResult<T> paginatedResult) where T : class
        {
            return Convert.ToInt32(Math.Ceiling((paginatedResult.RowCount / (decimal)paginatedResult.PageSize))); // 1 based page numbering
        }

        private bool HasNextPage<T>(IPaginatedResult<T> paginatedResult) where T: class
        {
            return paginatedResult.RowCount >
                paginatedResult.CurrentPage * paginatedResult.PageSize;
        }


        private string CreatePagedLinkRelToCurrentPage<T>(IPaginatedResult<T> paginatedResult, List<KeyValuePair<string, string>> filters, List<string> orderBy, IUrlHelper urlHelper,
            string pageRouteName, int pagesFromCurrentPage) where T : class
        {
            return CreatePageLink(pageSize: paginatedResult.PageSize,
                                   pageNumber: paginatedResult.CurrentPage + pagesFromCurrentPage,
                                   pageRouteName: pageRouteName,
                                   filters: filters,
                                   orderBy: orderBy,
                                   urlHelper: urlHelper);
        }

        private string CreatePageLink(int pageSize, int pageNumber, List<KeyValuePair<string, string>> filters, List<string> orderBy, string pageRouteName, IUrlHelper urlHelper)
        {
            // append the page and size query params to the generated URL.
            dynamic pageRouteValues = new ExpandoObject() as IDictionary<string, Object>; ;
            
            pageRouteValues.page = pageNumber;
            pageRouteValues.size = pageSize;

            // append any potential filter state to the generated URL by adding it to 'pageRouteValues'.
            foreach (var keyValuePair in filters)
            {
                ((IDictionary<string, Object>)pageRouteValues)[keyValuePair.Key] = keyValuePair.Value;
            }
            // generate the orderBy comma-separated array and append it to the generated URL by adding it to 'pageRouteValues'.
            if (orderBy != null && orderBy.Any()) {
                // Implode the orderBy list into a comma separated list.
                string commaSeparatedValues = string.Join(",", orderBy);

                pageRouteValues.orderBy = $"[{commaSeparatedValues}]";
            }            

            return urlHelper.Link(pageRouteName, pageRouteValues);
        }
    }
}
