using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanHangOnline.Models.EF;
namespace WebBanHangOnline.Models
{
    public class OrderAndDetailViewModel
    {
        public Order Order { get; set; }
        public OrderDetail OrderDetail { get; set; }
    }
}