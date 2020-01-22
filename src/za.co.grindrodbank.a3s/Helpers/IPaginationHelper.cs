using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Helpers
{
    public interface IPaginationHelper
    {
        void AddHeaderMetaData<T>(IPaginatedResult<T> paginatedResult, string pageRouteName,
            IUrlHelper urlHelper, HttpResponse response) where T : class;
    }
}
