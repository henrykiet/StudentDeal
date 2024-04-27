using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public HomeController()
        {
            _userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
        }

        public  ActionResult Index()
        {
            var products = db.Products.Where(p => p.IsActive == true && p.IsStatus == true).ToList();
            var topVouchers = db.Products.Where(t => t.IsHot == true &&t.IsActive == true && t.IsStatus == true).ToList();
            var news = db.News.Where(n => n.IsActive == true).Take(3).ToList();
            var viewModel = new ProductViewModel
            {
                Products = products,
                TopVouchers = topVouchers,
                productCategorys = db.ProductCategories.ToList(),
                News = news
            };



            //set wallet point của user vào session
            //lấy id user hiện tại
            var userId = User.Identity.GetUserId();
            if (userId != null)
            {
                //lấy name của user
                var name = _userManager.FindById(userId).FullName;
                Session["name"] = name;
                // Lấy danh sách role của người dùng hiện tại kiểm tra xem có phải admin hay không
                var roles =  _userManager.GetRoles(userId);
                bool isAdmin = roles.Contains("Admin");
                if (!isAdmin)
                {
                    //var currentUser = _userManager.FindById(userId);
                    var walletpoint = db.Wallets.Find(userId).point;
                    Session["walletpoint"] = walletpoint;
                }
            }
            return View(viewModel);
        }

        public ActionResult Partial_Subcrice()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Subscribe(Subscribe req)
        {
            if (ModelState.IsValid)
            {
                db.Subscribes.Add(new Subscribe { Email = req.Email, CreatedDate = DateTime.Now });
                db.SaveChanges();
                return Json(new {Success=true });
            }
            return View("Partial_Subcrice", req);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Refresh()
        {
            var item = new ThongKeModel();

            ViewBag.Visitors_online = HttpContext.Application["visitors_online"];
            var hn = HttpContext.Application["HomNay"];
            item.HomNay = HttpContext.Application["HomNay"].ToString();
            item.HomQua = HttpContext.Application["HomQua"].ToString();
            item.TuanNay = HttpContext.Application["TuanNay"].ToString();
            item.TuanTruoc = HttpContext.Application["TuanTruoc"].ToString();
            item.ThangNay = HttpContext.Application["ThangNay"].ToString();
            item.ThangTruoc = HttpContext.Application["ThangTruoc"].ToString();
            item.TatCa = HttpContext.Application["TatCa"].ToString();
            return PartialView(item);
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}