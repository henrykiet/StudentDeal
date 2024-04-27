using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Models
{
    public class ProductViewModel
    {
        public List<Product> Products { get; set; }
        public List<Product> TopVouchers { get; set; }
        public List<News> News { get; set; }
        public List<ProductCategory> productCategorys { get; set; }

    }
}