using System;
using System.Collections.Generic;
using System.Text;

namespace BalsamicSolutions.ApiCommon
{
    /// <summary>
    /// response class
    /// </summary>
    public class UserManagerResponse<T>
    {
        public bool Ok { get; set; }

        public string PaginationToken { get; set; }

        public IEnumerable<T> Results { get; set; }
    }
}
