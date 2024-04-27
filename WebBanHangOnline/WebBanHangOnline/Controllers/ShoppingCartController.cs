
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Helper;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController()
        {
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        }
        // GET: ShoppingCart
        public ActionResult Index()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }


        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }

        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult CheckOut(string id)
        {
            // Lấy thông tin người dùng hiện tại từ cơ sở dữ liệu
            ApplicationUser currentUser = db.Users.FirstOrDefault(u => u.Id == id);
            if (ModelState.IsValid)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart != null)
                {
                    if (currentUser != null)
                    {
                        // Kiểm tra xem người dùng có đủ điểm trong wallet để thanh toán không
                        Wallet userWallet = db.Wallets.FirstOrDefault(w => w.Id == currentUser.Id);
                        if (userWallet != null && userWallet.point >= cart.TotalAmount)
                        {
                            Order order = new Order();
                            int totalQuantity = 0;

                            cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
                            {
                                ProductId = x.ProductId,
                                Quantity = x.Quantity,
                                Price = (decimal)x.Price,
                            }));
                            Random rd = new Random();
                            var code = "";
                            var title = "";
                            // Xử lý thông báo lỗi và kết quả foreach
                            StringBuilder errorBuilder = new StringBuilder();
                            var count = 0;
                            foreach (var i in cart.Items)
                            {
                                totalQuantity += i.Quantity;
                                var item = db.Products.Find(i.ProductId);
                                if (item.Quantity <= 0)
                                {
                                    errorBuilder.AppendLine("voucher" + item.Title + " đã hết!");
                                }
                                var hasOrderedProduct = db.OrderDetails.Any(od => od.Order.ApplicationUserId == currentUser.Id && od.ProductId == item.Id);
                                if (hasOrderedProduct)
                                {
                                    errorBuilder.AppendLine("Bạn đã có voucher " + item.Title + ", hãy kiểm tra ví");
                                }
                                item.Quantity -= 1;
                                if (count > 0)
                                {
                                    code += " || " + "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                                    title += " || " + item.Title ;
                                }
                                else
                                {
                                    code += "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                                    title += item.Title;
                                }
                                count++;
                            }
                            // Nếu có thông báo lỗi, gán vào TempData và trả về kết quả thông báo
                            if (errorBuilder.Length > 0)
                            {
                                TempData["error"] = errorBuilder.ToString();
                                return Json(new { Success = false, Message = errorBuilder.ToString(), Code = -1 });
                            }

                            order.CustomerName = currentUser.FullName;
                            order.Phone = currentUser.Phone;
                            order.Email = currentUser.Email;
                            order.ApplicationUserId = currentUser.Id;
                            order.TypePayment = 2; // đã thanh toán
                            order.Quantity = totalQuantity;
                            order.TotalAmount = (decimal)cart.Items.Sum(x => (x.Price * x.Quantity));
                            order.CreatedDate = DateTime.Now;
                            order.ModifiedDate = DateTime.Now;
                            order.CreatedBy = currentUser.Phone;
                            order.Code = code;

                            db.Orders.Add(order);
                            db.SaveChanges();

                            userWallet.point -= cart.TotalAmount;
                            db.SaveChanges();

                            SendMail.SenMail(currentUser.Email, "Student Deal", "This is your voucher code: <span> " + order.Code + "</span>" + " use to for: " + title);

                            cart.ClearCart();

                            var result = new { Success = true, Message = "Thanh toán thành công. Hãy kiểm tra mail và ví của bạn" };
                            return Json(result);
                        }
                        else
                        {
                            var result = new { Success = false, Message = "Không đủ điểm trong wallet để thanh toán đơn hàng." };
                            return Json(result);
                        }
                    }
                }
            }

            var errorResult = new { Success = false, Message = "Thanh toán thất bại!" };
            return Json(errorResult);
        }




        [HttpPost]
        public ActionResult AddToCart(int id)
        {

            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            var db = new ApplicationDbContext();
            var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
            if (checkProduct != null)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart == null)
                {
                    cart = new ShoppingCart();
                }

                // Kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng chưa
                if (cart.Items.Any(x => x.ProductId == id))
                {
                    // Nếu sản phẩm đã tồn tại trong giỏ hàng, thì chỉ cập nhật số lượng lên 1
                    cart.UpdateQuantity(id, 1);
                }
                else
                {
                    // Nếu sản phẩm chưa tồn tại trong giỏ hàng, thì thêm mới với số lượng là 1
                    ShoppingCartItem item = new ShoppingCartItem
                    {
                        ProductId = checkProduct.Id,
                        ProductName = checkProduct.Title,
                        CategoryName = checkProduct.ProductCategory.Title,
                        Quantity = 1 // Số lượng mỗi lần thêm vào giỏ hàng là 1
                    };
                    if (checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault) != null)
                    {
                        item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault).Image;
                    }
                    item.Price = (float)checkProduct.Price;
                    if (checkProduct.PriceSale > 0)
                    {
                        item.Price = (float)checkProduct.PriceSale;
                    }
                    item.TotalPrice = item.Quantity * item.Price;
                    cart.AddToCart(item, 1); // Thêm sản phẩm vào giỏ hàng với số lượng là 1
                }

                Session["Cart"] = cart;
                code = new { Success = true, msg = "Thêm sản phẩm vào giỏ hàng thành công!", code = 1, Count = cart.Items.Count };
            }
            return Json(code);
        }



        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                // Kiểm tra nếu số lượng mới lớn hơn 1, thì giới hạn số lượng thành 1
                if (quantity > 1)
                {
                    quantity = 1;
                }
                cart.UpdateQuantity(id, quantity);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };

            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        private void SendOrderConfirmationEmail(Order order, string customerEmail)
        {
            // Địa chỉ email và mật khẩu của tài khoản gửi email
            string senderEmail = "your_email@gmail.com";
            string senderPassword = "your_email_password";

            // Tạo client SMTP để gửi email
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

            // Địa chỉ email của người nhận (khách hàng)
            MailAddress toAddress = new MailAddress(customerEmail);

            // Tạo đối tượng MailMessage để xây dựng email
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(senderEmail);
            mailMessage.To.Add(toAddress);
            mailMessage.Subject = $"Đơn hàng #{order.Code} - Xác nhận đơn hàng";
            mailMessage.IsBodyHtml = true;

            // Xây dựng nội dung email
            string emailContent = $"Xin chào {order.CustomerName},<br/><br/>" +
                $"Cảm ơn bạn đã đặt hàng từ chúng tôi. Dưới đây là chi tiết đơn hàng của bạn:<br/><br/>" +
                $"<table>" +
                $"<tr>" +
                $"<th>Tên sản phẩm</th>" +
                $"<th>Số lượng</th>" +
                $"<th>Giá</th>" +
                $"<th>Thành tiền</th>" +
                $"</tr>";

            decimal totalAmount = 0;
            foreach (var product in order.OrderDetails)
            {
                decimal totalPrice = product.Quantity * product.Price;
                totalAmount += totalPrice;

                emailContent +=
                    $"<tr>" +
                    $"<td>{product.Product.Title}</td>" +
                    $"<td>{product.Quantity}</td>" +
                    $"<td>{product.Price}</td>" +
                    $"<td>{totalPrice}</td>" +
                    $"</tr>";
            }

            emailContent +=
                $"</table><br/>" +
                $"Tổng tiền: {totalAmount}<br/><br/>" +
                $"Cảm ơn bạn đã mua hàng!<br/>";

            mailMessage.Body = emailContent;

            // Gửi email
            smtpClient.Send(mailMessage);
        }


        //get product
        [HttpPost]
        public ActionResult TakeProduct(int id)
        {
            // Lấy thông tin người dùng hiện tại từ HttpContext
            var userId = User.Identity.GetUserId();
            var currentUser = _userManager.FindById(userId);
            if (currentUser == null)
            {
                // Người dùng không được xác thực hoặc không tồn tại
                TempData["info"] = "Bạn phải đăng nhập để lấy voucher!";
                return Redirect("/Account/Login");
            }

            var item = db.Products.Find(id);
            if (item.Quantity <= 0)
            {
                //nếu voucher hết
                TempData["error"] = "voucher đã hết!";
                return Redirect("/Home/Index");
            }
            // Kiểm tra xem người dùng đã có order chưa và sản phẩm đó đã có trong order của họ chưa
            var hasOrderedProduct = db.OrderDetails.Any(od => od.Order.ApplicationUserId == currentUser.Id && od.ProductId == item.Id);
            if (hasOrderedProduct)
            {
                // Người dùng đã lấy voucher rồi 
                TempData["info"] = "Bạn đã có voucher này, hãy kiểm tra ví";
                return Redirect("/Home/Index");
            }


            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng có đủ điểm trong wallet để thanh toán không
                Wallet userWallet = db.Wallets.FirstOrDefault(w => w.Id == currentUser.Id);
                if (userWallet != null && userWallet.point >= (float)item.Price)
                {

                    //tạo order mới
                    Order order = new Order();
                    order.CustomerName = currentUser.FullName;
                    order.Phone = currentUser.Phone;
                    order.Email = currentUser.Email;
                    order.Quantity = 1;
                    order.ApplicationUserId = currentUser.Id;
                    order.TotalAmount = item.Price;
                    order.CreatedDate = DateTime.Now;
                    order.ModifiedDate = DateTime.Now;
                    order.TypePayment = 2;// đã thanh toán

                    //tạo code và bỏ vào order
                    Random rd = new Random();
                    order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);

                    //Thêm order và orderDetail vào context
                    db.Orders.Add(order);
                    //db.SaveChanges();
                    //tạo order detail trên order
                    OrderDetail orderDetail = new OrderDetail
                    {
                        ProductId = item.Id,
                        Quantity = 1,
                        Price = item.Price,
                        OrderId = order.OrderId
                    };
                    db.OrderDetails.Add(orderDetail);
                    //db.SaveChanges();

                    //trừ số lượng voucher đang có trong kho
                    item.Quantity -= 1;
                    //db.SaveChanges();

                    // Trừ điểm từ wallet của người dùng
                    userWallet.point = (float)userWallet.point - (float)item.Price;
                    //db.SaveChanges();

                    //Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    // Gửi email cho khách hàng và quản trị viên
                    //SendOrderConfirmationEmail(order);
                    SendMail.SenMail(currentUser.Email, "Student Deal", "This is your voucher code: <span style='font-weight:800;'>" + order.Code + "</span>" + " use to for: <span style='font-weight:800;'>" + item.Title + "</span>");
                    TempData["suc"] = "Nhận mã thành công hãy kiểm tra mail của bạn";
                    return RedirectToAction("Index", "Home");


                }
                else
                {
                    // Không đủ điểm trong wallet để thanh toán
                    TempData["war"] = "Không đủ điểm trong ví để thanh toán đơn hàng.";
                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["error"] = "Error";
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public ActionResult TakeProduct()
        {
            return View();
        }
    }



}



