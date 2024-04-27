using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Posts
        public ActionResult Index()
        {
            var items = db.Posts.ToList();
            return View(items);
        }
        public ActionResult Add()
        {
            var categories = db.ProductCategories.ToList();
            if (categories == null || categories.Count == 0)
            {
                ViewBag.ErrorMessage = "Category list is empty or null.";
                // Chuyển hướng đến hành động "Add" trong "CategoryController"
                TempData["war"] = "Bạn chưa có danh mục sản phẩm, hãy tạo mới danh mục!";

                return RedirectToAction("Add", "ProductCategory");
            }
            ViewBag.CategoryList = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Posts model)
        {
            if (ModelState.IsValid)
            {
                // Nếu mô hình không hợp lệ, thêm thông báo lỗi cho trường Title và trả về view
                ModelState.AddModelError("Title", "Vui lòng nhập tiêu đề.");
                // Lấy giá trị CategoryId đã chọn từ form
                int selectedCategoryId = model.ProductCategoryId;
                // Kiểm tra xem danh mục đã được chọn hay chưa
                if (selectedCategoryId != 0)
                {
                    // Lấy danh mục từ cơ sở dữ liệu dựa trên CategoryId
                    var selectedCategory = db.ProductCategories.FirstOrDefault(c => c.Id == selectedCategoryId);

                    // Gán danh mục đã chọn cho model
                    if (selectedCategory != null)
                    {
                        model.ProductCategory = selectedCategory;
                        model.ProductCategoryId = selectedCategory.Id;
                    }
                }
                model.IsActive = true;
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Posts.Add(model);
                TempData["suc"] = "Tạo mới bài viết thành công.";

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            // Khi xảy ra lỗi, gán danh sách Categories vào ViewBag.CategoryList
            ViewBag.CategoryList = db.ProductCategories.ToList();
            TempData["error"] = "Tạo mới bài viết không thành công.";

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var categories = db.ProductCategories.ToList();
            if (categories == null || categories.Count == 0)
            {
                ViewBag.ErrorMessage = "Category list is empty or null.";
                // Chuyển hướng đến hành động "Add" trong "ProductCategoryController"
                TempData["war"] = "Bạn chưa có danh mục sản phẩm, hãy tạo mới danh mục!";

                return RedirectToAction("Add", "ProductCategory");
            }

            ViewBag.CategoryList = new SelectList(categories, "Id", "Title");
            var item = db.Posts.Find(id);
            if (item != null)
            {
                // Lấy category tương ứng của post
                var selectedCategory = categories.FirstOrDefault(c => c.Id == item.ProductCategoryId);
                // Chỉ định giá trị được chọn cho dropdown
                ViewBag.SelectedCategoryId = selectedCategory?.Id;
                return View(item);
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Posts model)
        {
            if (ModelState.IsValid)
            {
                db.Posts.Attach(model);
                // Lấy giá trị CategoryId đã chọn từ form
                int selectedCategoryId = model.ProductCategoryId;
                // Kiểm tra xem danh mục đã được chọn hay chưa
                if (selectedCategoryId != 0)
                {
                    // Lấy danh mục từ cơ sở dữ liệu dựa trên CategoryId
                    var selectedCategory = db.ProductCategories.FirstOrDefault(c => c.Id == selectedCategoryId);

                    // Gán danh mục đã chọn cho model
                    if (selectedCategory != null)
                    {
                        model.ProductCategory = selectedCategory;
                        model.ProductCategoryId = selectedCategory.Id;
                    }
                }
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["suc"] = "Chỉnh sửa bài viết thành công.";

                return RedirectToAction("Index");
            }
            // Lấy lại danh sách categories và gán giá trị đã chọn cho dropdown
            var categories = db.ProductCategories.ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Title");
            ViewBag.SelectedCategoryId = model.ProductCategoryId;
            TempData["error"] = "Chỉnh sửa bài viết không thành công.";

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Posts.Find(id);
            if (item != null)
            {
                db.Posts.Remove(item);
                //item.IsActive = false;
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Posts.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        var obj = db.Posts.Find(Convert.ToInt32(item));
                        db.Posts.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


    }
}