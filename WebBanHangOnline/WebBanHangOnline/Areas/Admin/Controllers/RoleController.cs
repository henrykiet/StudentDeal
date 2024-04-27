using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Role
        public ActionResult Index()
        {
            var items = db.Roles.ToList();
            return View(items);
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                roleManager.Create(model);
                TempData["suc"] = "Thêm mới vai trò thành công";

                return RedirectToAction("Index");
            }
            TempData["error"] = "Thêm mới cai trò không thành công";

            return View(model);
        }
        public ActionResult Edit(string id)
        {
            var item = db.Roles.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

                // Lấy đối tượng vai trò hiện tại từ cơ sở dữ liệu
                var existingRole = roleManager.FindById(model.Id);

                if (existingRole != null)
                {
                    // Cập nhật thông tin của đối tượng hiện tại với thông tin từ model
                    existingRole.Name = model.Name;

                    // Thực hiện cập nhật vào cơ sở dữ liệu
                    var result = roleManager.Update(existingRole);

                    if (result.Succeeded)
                    {
                        TempData["suc"] = "Chỉnh sửa vai trò thành công";
                        return RedirectToAction("Index");
                    }
                }

                TempData["error"] = "Chỉnh sửa vai trò không thành công";
            }

            return View(model);
        }


    }
}