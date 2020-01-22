using System;
using System.Collections.Generic;

namespace za.co.grindrodbank.a3s.Repositories
{
    public interface IPaginatedResult
    {
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
        public IEnumerable<dynamic> Results { get; set; }
        public int FirstRowOnPage { get; }
        public int LastRowOnPage { get; }
    }
}
