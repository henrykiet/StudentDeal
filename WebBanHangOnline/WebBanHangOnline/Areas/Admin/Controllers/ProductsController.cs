using PagedList;
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
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Products
        public ActionResult Index(int? page)
        {
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult Add()
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        if (i + 1 == rDefault[0])
                        {
                            model.Image = Images[i];
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = true
                            });
                        }
                        else
                        {
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = false
                            });
                        }
                    }
                }

                if (model.Quantity < 0 || model.Quantity.Equals(""))
                {
                    ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                    TempData["error"] = "Số lượng phải lớn hơn 0, nhỏ hơn 10 và không được bỏ trống";
                    return View(model);
                }
                if (model.Price < 0 || model.Price.Equals(""))
                {
                    ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                    TempData["error"] = "Giá phải lớn hơn 0 và không được để trống";
                    return View(model);
                }
                if (model.StartDate < DateTime.Now.AddDays(-3))
                {
                    ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                    TempData["error"] = "Ngày bắt đầu chỉ được trước ngày hiện tại 3 ngày";
                    return View(model);
                }
                if (model.endDate <= model.StartDate || model.endDate < DateTime.Now)
                {
                    ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                    TempData["error"] = "Ngày kết thúc không được trước ngày hiện tại và ngày bắt đầu";
                    return View(model);
                }


                model.TotalQuantity = model.Quantity;
                model.IsStatus = true;
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                if (string.IsNullOrEmpty(model.SeoTitle))
                {
                    model.SeoTitle = model.Title;
                }
                db.Products.Add(model);
                db.SaveChanges();
                TempData["suc"] = "Thêm mới sản phẩm thành công";
                return RedirectToAction("Index");
            }
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            TempData["error"] = "Thêm mới sản phẩm không thành công";

            return View(model);
        }


        public ActionResult Edit(int id)
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            var item = db.Products.Find(id);
            if (item != null)
            {
                // Lấy dữ liệu từ cơ sở dữ liệu
                item.StartDate = (DateTime)(db.Products.FirstOrDefault(x => x.Id == id)?.StartDate);
                item.endDate = (DateTime)(db.Products.FirstOrDefault(x => x.Id == id)?.endDate);

                return View(item);
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                    if(model.Quantity < 0 || model.Quantity.Equals(" "))
                    {
                        ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                        TempData["error"] = "Số lượng phải lớn hơn 0, nhỏ hơn 10 và không được bỏ trống";
                        return View(model);
                    }
                    if (model.Price < 0 || model.Price.Equals(" "))
                    {
                        ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                        TempData["error"] = "Giá phải lớn hơn 0 và không được để trống";
                        return View(model);
                    }
                    if (model.StartDate < DateTime.Now.AddDays(-3))
                    {
                        ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                        TempData["error"] = "Ngày bắt đầu chỉ được trước ngày hiện tại 3 ngày";
                        return View(model);
                    }
                    if (model.endDate <= model.StartDate || model.endDate < DateTime.Now)
                    {
                        ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                        TempData["error"] = "Ngày kết thúc không được trước ngày hiện tại và ngày bắt đầu";
                        return View(model);
                    }
                model.ModifiedDate = DateTime.Now;
                db.Products.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["suc"] = "Chỉnh sửa sản phẩm thành công";

                return RedirectToAction("Index");
            }
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            TempData["error"] = "Chỉnh sửa sản phẩm không thành công";
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                //db.Products.Remove(item);
                item.IsStatus = false;
                db.SaveChanges();
                TempData["suc"] = "Xóa sản phẩm thành công";

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Products.Find(id);
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
        public ActionResult IsHome(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsHome = !item.IsHome;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsHome = item.IsHome });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsHot(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsHot = !item.IsHot;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsHot = item.IsHot });
            }

            return Json(new { success = false });
        }
    }
}