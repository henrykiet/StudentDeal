using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/News
        public ActionResult Index(string Searchtext, int? page)
        {

            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<News> items = db.News.OrderByDescending(x => x.Id).Include(x => x.Category);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }
        [HttpGet]
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
        public ActionResult Add(News model)
        {
            if (ModelState.IsValid)
            {
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
                db.News.Add(model);
                db.SaveChanges();
                TempData["suc"] = "Tạo mới tin tức thành công.";
                return RedirectToAction("Index");

            }
            // Khi xảy ra lỗi, gán danh sách Categories vào ViewBag.CategoryList
            ViewBag.CategoryList = db.ProductCategories.ToList();
            TempData["error"] = "Tạo mới tin tức không thành công.";

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var categories = db.ProductCategories.ToList();
            if (categories == null || categories.Count == 0)
            {
                ViewBag.ErrorMessage = "Product Category list is empty or null.";
                // Chuyển hướng đến hành động "Add" trong "CategoryController"
                TempData["war"] = "Bạn chưa có danh mục sản phẩm, hãy tạo mới danh mục!";
                return RedirectToAction("Add", "ProductCategory");
            }

            ViewBag.CategoryList = new SelectList(categories, "Id", "Title");
            var item = db.News.Find(id);
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
        public ActionResult Edit(News model)
        {
            if (ModelState.IsValid)
            {
                db.News .Attach(model);
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
                db.Entry(model).State = EntityState.Modified; // Đánh dấu đối tượng là đã thay đổi
                db.SaveChanges();
                TempData["suc"] = "Chỉnh sửa bài viết thành công.";

                return RedirectToAction("Index");
            }
            // Lấy lại danh sách categories và gán giá trị đã chọn cho dropdown
            var categories = db.ProductCategories.ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Title");
            ViewBag.SelectedCategoryId = model.ProductCategoryId;
            TempData["error"] = "Chỉnh sửa tin tức không thành công.";

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.News.Find(id);
            if (item != null)
            {
                db.News.Remove(item);
                //item.IsActive = false;
                TempData["suc"] = "Xóa tin tức thành công.";
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.News.Find(id);
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
                        var obj = db.News.Find(Convert.ToInt32(item));
                        db.News.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

    }
}