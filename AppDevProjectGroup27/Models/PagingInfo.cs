using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models
{
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int ItemPerPage { get; set; }
        public int CurrentPage { get; set; }

        public int Totalpage => (int)Math.Ceiling((decimal)TotalItems / ItemPerPage);

        public string UrlParam { get; set; }
    }
}
