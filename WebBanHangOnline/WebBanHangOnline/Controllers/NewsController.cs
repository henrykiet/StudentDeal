using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: News
        public ActionResult Index()
        {
            var productcategory = db.ProductCategories.Where(p => p.IsActive == true).ToList();
            var news = db.News.Where(n => n.IsActive == true).ToList();
            var top3 = db.News.Take(3).ToList();
            NewsProductCateViewModel viewmodel = new NewsProductCateViewModel
            {
                News = news,
                ProductCategory = productcategory,
                Top3 = top3
            };
            return View(viewmodel);
        }
        public ActionResult Detail(int id)
        {
            var item = db.News.Find(id);
            return View(item);
        }
        public ActionResult Partial_News_Home()
        {
            var items = db.News.Where(n => n.IsActive == true).ToList();
            return PartialView(items);
        }
    }
}