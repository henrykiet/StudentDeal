using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebBanHangOnline.Helper;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    [Authorize]
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
        public async Task<ActionResult> Login(LoginRegisterViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.LoginModel.UserName);
            if (user == null)
            {
                TempData["war"] = "Không tìm thấy người dùng với tên đăng nhập đã cung cấp";
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(user.UserName, model.LoginModel.Password, model.LoginModel.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if (!user.EmailConfirmed)
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        return View("NotificationEmailConfirm");
                    }
                    else if (user.IsActive == false)
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        TempData["error"] = "Tài khoản đã bị khóa hãy liên hệ với quản trị viên";
                        return Redirect("/Account/Login");
                    }
                    else
                    {
                        TempData["suc"] = "Đăng nhập thành công";
                        // Đặt lại số lần đăng nhập sai về 0 khi đăng nhập thành công
                        user.FailedLoginAttempts = 0;
                        await UserManager.UpdateAsync(user);
                        return RedirectToLocal(returnUrl);
                    }

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.LoginModel.RememberMe });

                case SignInStatus.Failure:
                    // Tăng số lần đăng nhập sai và cập nhật người dùng
                    user.FailedLoginAttempts++;
                    await UserManager.UpdateAsync(user);

                    // Kiểm tra nếu người dùng đã đăng nhập sai 5 lần, thì khóa tài khoản bằng cách đặt IsActive thành false
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsActive = false;
                        await UserManager.UpdateAsync(user);

                        TempData["error"] = "Tài khoản đã bị khóa sau khi đăng nhập sai 5 lần. Hãy liên hệ với quản trị viên.";
                        return Redirect("/Account/Login");
                    }

                    TempData["error"] = "Sai tên người dùng hoặc mật khẩu. Sai 5 lần sẽ bị khóa";
                    return View(model);

                default:
                    TempData["error"] = "Lỗi đăng nhập không hợp lệ";
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(LoginRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //if(model.RegisterModel.Email)
                var user = new ApplicationUser { UserName = model.RegisterModel.Email, Email = model.RegisterModel.Email, FullName = model.RegisterModel.FullName, Gender = model.RegisterModel.Gender, IsActive = true };
                var result = await UserManager.CreateAsync(user, model.RegisterModel.Password);
                if (result.Succeeded)
                {
                    var wallet = new Wallet { Id = user.Id, point = 100, status = true };
                    user.Wallet = wallet;
                    wallet.CreatedDate = DateTime.Now;
                    wallet.ModifiedDate = DateTime.Now;
                    db.Wallets.Add(wallet);
                    db.SaveChanges();
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);


                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    SendMail.SenMail(user.Email, "Student Deal", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    //set return home/index và in ra thông báo phải xác nhận email
                    TempData["info"] = "PLease, bạn hãy xác nhận tài khoản trong mail của bạn";
                    return RedirectToAction("Login");

                }
                AddErrors(result);
            }
            TempData["war"] = "Email đã tồn tại";
            // If we got this far, something failed, redisplay form
            return RedirectToAction("Login");
        }
        public new ActionResult Profile()
        {
            // Lấy thông tin người dùng hiện tại
            var userId = User.Identity.GetUserId();
            var user = UserManager.FindById(userId);
            var walletPoint = (float)0;
            ProfileViewModel viewModel;
            if (user.Wallet == null)
            {
                viewModel = new ProfileViewModel
                {
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Gender = user.Gender,
                    WalletPoint = 0
                };
            }
            else
            {
                // Kiểm tra xem user có thuộc tính Wallet hay không
                if (user.Wallet.status == true)
                {
                    walletPoint = user.Wallet.point;
                }
                else
                {
                    walletPoint = 0;
                }

                viewModel = new ProfileViewModel
                {
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Gender = user.Gender,
                    WalletPoint = walletPoint
                };
            }
            return View(viewModel);
        }

        // Action để cập nhật hồ sơ
        [HttpPost]
        public ActionResult UpdateProfile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Lấy thông tin người dùng hiện tại
                var userId = User.Identity.GetUserId();
                var user = UserManager.FindById(userId);

                // Kiểm tra xem tên người dùng có tồn tại hay không
                bool isUsernameExist = db.Users.Any(u => u.UserName == model.UserName && u.Id != userId);

                // Cập nhật thông tin hồ sơ từ model
                if (!isUsernameExist)
                {
                    user.UserName = model.UserName;
                }
                else
                {
                    TempData["error"] = "Tên đăng nhập đã tồn tại";
                    return View("Profile", model);
                }
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.Phone;
                user.Gender = model.Gender;

                // Lưu thay đổi vào cơ sở dữ liệu
                var result = UserManager.Update(user);

                if (result.Succeeded)
                {
                    // Cập nhật thành công, arlert
                    TempData["suc"] = "Cập nhập hồ sơ thành công";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["error"] = "Cập nhập hồ sơ không thành công";

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            TempData["error"] = "Cập nhật hồ sơ không thành công.";
            // Nếu ModelState không hợp lệ
            return View("Profile", model);
        }


        [HttpPost]
        public ActionResult UpdatePassword(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);

                if (user == null)
                {
                    // Xử lý khi không tìm thấy người dùng
                    return Redirect("Profile");

                }

                // Kiểm tra xem mật khẩu cũ nhập vào có khớp với mật khẩu của người dùng hay không
                var passwordHasher = new PasswordHasher();
                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user.PasswordHash, model.Password);

                if (passwordVerificationResult != PasswordVerificationResult.Success)
                {
                    // Xử lý khi mật khẩu cũ nhập vào không khớp với mật khẩu của người dùng
                    TempData["error"] = "Mật khẩu cũ không đúng";
                    return Redirect("Profile");


                }

                if (model.NewPassword != model.RePassword)
                {
                    // repassword không khớp
                    TempData["error"] = "Mật khẩu xác nhận không đúng";
                    return Redirect("Profile");
                }

                // Tiến hành thay đổi mật khẩu mới
                var newPasswordHash = passwordHasher.HashPassword(model.NewPassword);
                user.PasswordHash = newPasswordHash;
                var updateResult = db.SaveChanges();

                if (updateResult > 0)
                {
                    TempData["suc"] = "Cập nhật mật khẩu thành công";
                    return RedirectToAction("Index", "Home");
                }

                TempData["error"] = "Có lỗi xảy ra khi cập nhật mật khẩu";
            }

            // Khi ModelState không hợp lệ hoặc có lỗi xảy ra, hiển thị lại trang với thông tin mô hình (model) và lỗi nếu có
            return View("Profile", model);
        }




        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                SendMail.SenMail(user.Email, "Student Deal", "Please  clicking <a href=\"" + callbackUrl + "\">here</a> to resetpassword");

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
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

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion


    }
}