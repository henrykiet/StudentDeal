using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Category
        public ActionResult Index()
        {
            var items = db.Categories;
            return View(items);
        }
        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Category model)
        {
            if (ModelState.IsValid)
            {
                var isPositionExist = db.Categories.Any(c => c.Position == model.Position);

                if (isPositionExist)
                {
                    ModelState.AddModelError("Position", "Giá trị Position đã tồn tại.");
                    return View(model);
                }

                if(model.Position < 0 || model.Position.Equals(" "))
                {
                    TempData["error"] = "Position không được để trống và phải lớn hơn 0";
                    return View(model);
                }

                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                if(model.Alias == null || model.Alias.Equals(" "))
                {
                    model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);//link đến controller
                }
                model.IsActive = true;
                db.Categories.Add(model);
                db.SaveChanges();
                TempData["suc"] = "Thêm danh mục thành công";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var item = db.Categories.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category model)
        {
            if (ModelState.IsValid)
            {
                var position = db.Categories.Find(model.Id);

                // Kiểm tra xem có bất kỳ đối tượng nào khác có cùng Position nhưng khác với đối tượng hiện tại không
                var isPositionExist = db.Categories
                    .Where(c => c.Position == model.Position && c.IsActive == true && c.Id != model.Id)
                    .Any();

                if (isPositionExist)
                {
                    ModelState.AddModelError("Position", "Giá trị Position đã tồn tại.");
                    return View(model);
                }
                if (model.Position < 0 || model.Position.Equals(" "))
                {
                    TempData["error"] = "Position không được để trống và phải lớn hơn 0";
                    return View(model);
                }
                // Cập nhật các trường
                position.Title = model.Title;
                position.Description = model.Description;
                position.Alias = model.Alias; //WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                position.SeoDescription = model.SeoDescription;
                position.SeoKeywords = model.SeoKeywords;
                position.SeoTitle = model.SeoTitle;
                position.Position = model.Position;
                position.IsActive = model.IsActive;
                position.ModifiedDate = DateTime.Now;
                position.Modifiedby = model.Modifiedby;
                db.SaveChanges();
                TempData["suc"] = "Sửa danh mục thành công";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Categories.Find(id);
            if (item != null)
            {
                //var DeleteItem = db.Categories.Attach(item);
                item.IsActive = false;
                //db.Categories.Remove(item);
                TempData["info"] = "Đổi danh mục thành công";
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

    }
}