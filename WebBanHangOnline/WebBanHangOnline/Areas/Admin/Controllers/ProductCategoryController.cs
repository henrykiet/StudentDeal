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
    public class ProductCategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductCategory
        public ActionResult Index()
        {
            var items = db.ProductCategories.ToList();
            return View(items);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ProductCategory model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Title))
                {
                    TempData["error"] = "tiêu đề không được bỏ trống";
                    return View(model);
                }
                if (string.IsNullOrEmpty(model.Alias))
                {
                    TempData["error"] = "đường link không được để trống";
                    return View(model);
                }
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.IsActive = true;
                db.ProductCategories.Add(model);
                db.SaveChanges();
                TempData["suc"] = "Thêm mới danh mục sản phẩm thành công";
                return RedirectToAction("Index");
            }
            return View();
        }
        public ActionResult Edit(int id)
        {
            var item = db.ProductCategories.Find(id);
            
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductCategory model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Title))
                {
                    TempData["error"] = "tiêu đề không được bỏ trống";
                    return View(model);
                }
                if (string.IsNullOrEmpty(model.Alias))
                {
                    TempData["error"] = "đường link không được để trống";
                    return View(model);
                }
                model.ModifiedDate = DateTime.Now;
                db.ProductCategories.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                TempData["suc"] = "Chỉnh sửa danh mục sản phẩm thành công";

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            TempData["error"] = "Chỉnh sửa danh mục sản phẩm không thành công";
            return View();
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.ProductCategories.Find(id);
            if (item != null)
            {
                //db.ProductCategories.Remove(item);
                item.IsActive = false;
                TempData["suc"] = "Xóa danh mục sản phẩm thành công";

                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}