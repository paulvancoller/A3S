using System;
using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Repositories
{
    public interface IPaginatedResult<T> where T : class
    {
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
        public IList<T> Results { get; set; }
        public int FirstRowOnPage { get; }
        public int LastRowOnPage { get; }
    }
}
