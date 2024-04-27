using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Models
{
    public class NewsProductCateViewModel
    {
        public List<News> News { get; set; }
        public List<News> Top3 { get; set; }

        public List<ProductCategory> ProductCategory{get;set;}
    }
}