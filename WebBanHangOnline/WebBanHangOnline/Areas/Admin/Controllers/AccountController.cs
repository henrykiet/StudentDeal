using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Admin/Account
        public ActionResult Index()
        {
            var ítems = db.Users.ToList();
            return View(ítems);
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    TempData["suc"] = "Đăng nhập thành công";
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                    TempData["error"] = "Sai tên tài khoản hoặc mật khẩu!";
                    return RedirectToAction("Login");
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            TempData["suc"] = "Đăng xuất thành công";
            return RedirectToAction("Index", "Home");
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Create()
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    TempData["error"] = "Email không được bỏ trống";
                    return View(model);
                }
                if (string.IsNullOrEmpty(model.FullName))
                {
                    TempData["error"] = "Fullname không được để trống";
                    return View(model);
                }
                // Kiểm tra xem tên người dùng có tồn tại hay không
                bool isUsernameExist = db.Users.Any(u => u.UserName == model.UserName);

                // Cập nhật thông tin hồ sơ từ model
                if (isUsernameExist)
                {
                    TempData["error"] = "Tên đăng nhập đã tồn tại";
                    return View(model);
                }
                var user = new ApplicationUser
                {
                    //Roles = model.Role,
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    IsActive = true
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, model.Role);
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    // Gán vai trò cho người dùng mới tạo
                    await UserManager.AddToRoleAsync(user.Id, model.Role);

                    TempData["suc"] = "Thêm mới người dùng thành công";

                    return RedirectToAction("Index", "Account");
                }

                AddErrors(result);
            }
                TempData["error"] = "Thêm mới người dùng không thành công";
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpGet]
        public ActionResult Edit(string id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                var profileModel = new EditAccountViewModel
                {
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Gender = user.Gender,
                    IsActive = user.IsActive,
                    Role = db.Roles.FirstOrDefault(r => r.Users.Any(u => u.UserId == user.Id))?.Name // Lấy vai trò của người dùng nếu có
                };

                ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");

                return View(profileModel);
            }

            // Nếu không tìm thấy người dùng, bạn có thể xử lý lỗi ở đây hoặc chuyển hướng đến trang không tìm thấy.
            return RedirectToAction("NotFound");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(model.Email) )
                    {
                        TempData["error"] = "Email không được bỏ trống";
                        return View(model);
                    }
                    if (string.IsNullOrEmpty(model.FullName))
                    {
                        TempData["error"] = "Fullname không được để trống";
                        return View(model);
                    }
                    // Kiểm tra xem tên người dùng có tồn tại hay không
                    bool isUsernameExist = db.Users.Any(u => u.UserName == model.UserName);

                    // Cập nhật thông tin hồ sơ từ model
                    if (isUsernameExist)
                    {
                        TempData["error"] = "Tên đăng nhập đã tồn tại";
                        return View(model);
                    }

                    user.UserName = model.UserName;
                    user.FullName = model.FullName;
                    user.Phone = model.Phone;
                    user.Gender = model.Gender;
                    user.Email = model.Email;
                    user.IsActive = model.IsActive;
                    if(model.IsActive == true)
                    {
                        user.FailedLoginAttempts = 0;
                    }
                    else
                    {
                        user.FailedLoginAttempts = 5;
                    }
                    // Cập nhật thông tin vào cơ sở dữ liệu
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    // Lấy ra vai trò đã chọn từ model.Role
                    var selectedRole = model.Role;

                    // Kiểm tra xem vai trò đã chọn có tồn tại trong cơ sở dữ liệu hay không
                    if (db.Roles.Any(r => r.Name == selectedRole))
                    {
                        // Xóa tất cả các vai trò hiện tại của người dùng (nếu có)
                        var currentRoles = await UserManager.GetRolesAsync(user.Id);
                        await UserManager.RemoveFromRolesAsync(user.Id, currentRoles.ToArray());

                        // Gán vai trò mới cho người dùng
                        await UserManager.AddToRoleAsync(user.Id, selectedRole);
                    }

                    await db.SaveChangesAsync();

                    // Thực hiện xử lý sau khi cập nhật thành công
                    TempData["suc"] = "Cập nhật người dùng thành công";

                    // Chuyển hướng về trang Index để hiển thị danh sách người dùng sau khi cập nhật
                    return RedirectToAction("Index");
                }
            }

            // Nếu ModelState không hợp lệ, hiển thị lại view Edit với thông tin người dùng và các thông báo lỗi
            TempData["error"] = "Cập nhật người dùng không thành công";

            // Khi gặp lỗi, bạn cũng cần gán lại danh sách vai trò vào ViewBag để dropdownlist có dữ liệu để hiển thị
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            var currentUserId = User.Identity.GetUserId();

            if (user == null)
            {
                TempData["error"] = "Người dùng không tồn tại.";
                return RedirectToAction("Index");
            }

            if (user.Id == currentUserId)
            {
                // Trying to delete their own account, show an error message and redirect to the index page
                TempData["error"] = "Bạn không thể xóa chính mình!!!";
                return RedirectToAction("Index");
            }
            //đổi trạng thái order
            user.IsActive = false;
            //db.Users.Remove(user);
            await db.SaveChangesAsync();

            // Successful deletion, show a success message and redirect to the index page
            TempData["suc"] = "Xóa thành công";
            return RedirectToAction("Index");
        }



        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}