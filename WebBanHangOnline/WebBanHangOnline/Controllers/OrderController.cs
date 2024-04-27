using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Helper;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    public class OrderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public OrderController()
        {
            _userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
        }

        // GET: Order
        public ActionResult Index()
        {
            // Lấy thông tin người dùng hiện tại từ HttpContext
            var userId = User.Identity.GetUserId();
            var currentUser = db.Users.Find(userId);

            // Lấy danh sách các đơn hàng của người dùng hiện tại kèm theo thông tin sản phẩm
            var userOrders = db.Orders
                .Where(o => o.ApplicationUserId == currentUser.Id)
                .Include(o => o.OrderDetails.Select(od => od.Product)) // Thêm Include để lấy thông tin sản phẩm
                .ToList();

            // Tạo danh sách ViewModel để lưu thông tin từ cả hai bảng
            var orderViewModels = new List<UserOrderViewModel>();

            foreach (var order in userOrders)
            {
                // Tạo một ViewModel mới để lưu trữ thông tin từ cả hai bảng
                var orderViewModel = new UserOrderViewModel
                {
                    Order = order,
                    OrderDetails = order.OrderDetails.ToList() // Không cần lấy thông tin sản phẩm ở đây vì đã có Include() ở trên
                };

                // Thêm ViewModel vào danh sách
                orderViewModels.Add(orderViewModel);
            }

            // Truyền danh sách ViewModel vào View để hiển thị thông tin
            return View(orderViewModels);
        }
        [HttpPost]
        public ActionResult Recode(int id)
        {

            //lấy id user hiện tại
            var userId = User.Identity.GetUserId();
            var user = _userManager.FindById(userId);

            var od = db.Orders.Find(id);
            var odDetail = db.OrderDetails.FirstOrDefault(o => o.OrderId == od.OrderId);
            var product = db.Products.FirstOrDefault(p => p.Id == odDetail.ProductId);
            var code = od.Code;
            SendMail.SenMail(user.Email, "Student Deal", "This code: " + code + " use to: " + product.Title);
            TempData["suc"] = "Đã gửi mã thành công hãy kiểm tra mail của bạn.";
            return RedirectToAction("Index", "Home");
        }
    }
}