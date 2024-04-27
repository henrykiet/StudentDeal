using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products
        public ActionResult Index()
        {
            // Lấy ProductViewModel từ TempData
            var viewModel = TempData["ProductViewModel"] as ProductViewModel;

            // Kiểm tra viewModel có tồn tại hay không
            if (viewModel != null)
            {
                return View(viewModel);
            }

            // Trong trường hợp không có viewModel, xử lý bình thường
            var products = db.Products.Where(p => p.IsActive == true && p.IsStatus == true).ToList();
            viewModel = new ProductViewModel
            {
                Products = products,
                productCategorys = db.ProductCategories.ToList()
            };

            return View(viewModel);
        }


        [HttpGet]
        public ActionResult Search(string search)
        {

            var products = db.Products.ToList();
            var productCategory = db.ProductCategories.ToList();
            var viewModel = new ProductViewModel();
            if (!string.IsNullOrEmpty(search))
            {
                var matchingCategories = productCategory.Where(pc => pc.Title.ToLower().Contains(search.ToLower())).ToList();
                if (matchingCategories.Count == 0) // Kiểm tra xem có kết quả tìm kiếm theo Title của ProductCategory không
                {
                    products = products.Where(p => p.Title.ToLower().Contains(search.ToLower())).ToList();
                    if (products != null)
                    {
                        productCategory = db.ProductCategories.ToList();
                    }
                }
                else
                {
                    // Nếu tìm thấy kết quả tìm kiếm theo Title của ProductCategory
                    var matchingProductIds = matchingCategories.SelectMany(pc => pc.Products.Select(p => p.Id)).ToList();
                    products = products.Where(p => matchingProductIds.Contains(p.Id)).ToList();
                }
            }
            viewModel.Products = products;
            viewModel.productCategorys = productCategory;

            TempData["ProductViewModel"] = viewModel;
            return RedirectToAction("Index", "Products");
        }

        public ActionResult Detail(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                db.Products.Attach(item);
                item.ViewCount = item.ViewCount + 1;
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();
            }

            return View(item);
        }
        public ActionResult ProductCategory(string alias, int id)
        {
            var items = db.Products.ToList();
            if (id > 0)
            {
                items = items.Where(x => x.ProductCategoryId == id).ToList();
            }
            var cate = db.ProductCategories.Find(id);
            if (cate != null)
            {
                ViewBag.CateName = cate.Title;
            }

            ViewBag.CateId = id;
            return View(items);
        }
        public ActionResult Partial_ItemsByCateId(int? id)
        {
            // Kiểm tra nếu id không null và hợp lệ
            if (id != null && id > 0)
            {
                var items = db.Products.Where(x => x.IsHome && x.IsActive && x.ProductCategoryId == id).Take(12).ToList();
                return PartialView(items);
            }

            // Trả về một danh sách trống nếu id không hợp lệ hoặc null
            return PartialView(new List<Product>());
        }

        public ActionResult Partial_ProductSales()
        {
            var items = db.Products.Where(x => x.IsSale && x.IsActive).Take(12).ToList();
            return PartialView(items);
        }

        [HttpPost]
        public ActionResult AddReview(int productId, int rating, string commentContent)
        {
            // Kiểm tra người dùng đã đăng nhập hay chưa
            if (!User.Identity.IsAuthenticated)
            {
                // Nếu chưa đăng nhập, trả về mã lỗi (có thể là mã lỗi JSON hoặc mã lỗi HTTP 401)
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh giá sản phẩm!" });
            }

            // Tạo một đối tượng Review mới
            var review = new Review
            {
                rating = rating,
                comment = commentContent,
                DateVote = DateTime.Now,
                ProductId = productId,
                UserId = User.Identity.GetUserId() // Sử dụng thư viện Microsoft.AspNet.Identity để lấy ID của người dùng đăng nhập
            };

            // Lưu đánh giá vào cơ sở dữ liệu
            db.Reviews.Add(review);
            db.SaveChanges();

            // Trả về mã thành công (có thể là mã lỗi JSON hoặc mã lỗi HTTP 200)
            return Json(new { success = true });
        }
    }
}