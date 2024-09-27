using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using X.PagedList;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.Rest.Verify.V2.Service;
using DNTCaptcha.Core;
using static Microsoft.ClearScript.V8.V8CpuProfile;
using Demo.Models;


namespace Demo.Controllers;

public class AccountController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;
    private readonly IConfiguration cf;


    public AccountController(DB db, IWebHostEnvironment en, Helper hp,IConfiguration cf)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
        this.cf = cf;
    }

    // GET: Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Account/Login
    [HttpPost, ValidateAntiForgeryToken]
    [ValidateDNTCaptcha(ErrorMessage = "Wrong Code. Pls Enter Again!!",
                    CaptchaGeneratorLanguage = Language.English,
                    CaptchaGeneratorDisplayMode = DisplayMode.SumOfTwoNumbersToWords)]

    public IActionResult Login(LoginVM vm, string? returnURL)
    {
        var u = db.Users.Find(vm.Email);
        
        if (u == null || !hp.VerifyPassword(u.Hash, vm.Password))
        {
            ModelState.AddModelError("", "Login Credentials Not Matched.");
        }

        if (ModelState.IsValid)
        {

            TempData["Info"] = "Login Successfully.";

            hp.SignIn(u!.Email, u.Role, vm.RememberMe);

            if (u is Student s)
            {
                HttpContext.Session.SetString("PhotoURL", s.PhotoURL);
            }

            if (string.IsNullOrEmpty(returnURL))
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        return View(vm);
    }

    // GET: Account/Logout
    public IActionResult Logout(string? returnURL)
    {
        TempData["Info"] = "Logout Successfully.";

        hp.SignOut();

        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }

    // GET: Account/AccessDenied
    public IActionResult AccessDenied(string? returnURL)
    {
        return View();
    }



    // ------------------------------------------------------------------------
    // Others
    // ------------------------------------------------------------------------

    // GET: Account/CheckEmail
    public bool CheckEmail(string email)
    {
        return !db.Users.Any(u => u.Email == email);
    }

    // GET: Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    public IActionResult Register(RegisterVM vm)
    {
        if (ModelState.IsValid("Email") && db.Users.Any(u => u.Email == vm.Email))
        {
            ModelState.AddModelError("Email", "Duplicated Email.");
        }

        if (ModelState.IsValid("Photo"))
        {
            var err = hp.ValidatePhoto(vm.Photo);
            if (err != "") ModelState.AddModelError("Photo", err);
        }

        if (ModelState.IsValid)
        {
            var code = Helper.RandomString(6);

            var newUser = new User
            {
                Email = vm.Email,
                Name = vm.Name,
                Gender = vm.Gender,
                PhoneNumber = vm.Phone,
                Address = vm.Address,
                Year = vm.IntakeYear,
                Faculty_Code = vm.Faculty,
                Programme_Code = vm.Programme,
                Hash = hp.HashPassword(vm.Password),
                PhotoURL = hp.SavePhoto(vm.Photo),
                Status = false,
                SecurityCode = code,
                CreationDate = DateTime.Now,

                Role = "S"

            };

            var newStudent = new Student
            {
                Email = newUser.Email,
                Name = newUser.Name,
                Gender = newUser.Gender,
                PhoneNumber = newUser.PhoneNumber,
                Address = newUser.Address,
                Year = vm.IntakeYear,
                Faculty_Code = vm.Faculty,
                Programme_Code = vm.Programme,
                Hash = newUser.Hash,
                Role = newUser.Role,
                PhotoURL = hp.SavePhoto(vm.Photo),
                Status = false,
                SecurityCode = code,
                CreationDate = DateTime.Now,

            };
            db.Students.Add(newStudent);
            
            db.SaveChanges();

            SendSMS(vm.Phone, code);

            //TempData["Info"] = "Register successfully.";
            TempData["Email"] = vm.Email;
            TempData["Phone"] = vm.Phone;
            return RedirectToAction("Active");

        }

        return View();
    }

    private void SendSMS(string p, string c)
    {
        // TODO: Twilio - Setup
        var sid = cf["Twilio:SID"];
        var token = cf["Twilio:Token"];
        var phone = cf["Twilio:Phone"];

        TwilioClient.Init(sid, token);

        // TODO: Twilio - Send SMS
        try
        {
            var message = MessageResource.Create(
                from: new PhoneNumber(phone),
                to: new PhoneNumber(p),
                body: c
                );
            //TempData["Info"] = "Register Successfully.";
        }
        catch (TwilioException ex)
        {
            TempData["Info"] = $"ERROR:{ex.Message}";
        }
    }

    // GET: Account/Active
    public IActionResult Active()
    {
        
        if (Request.Headers.Referer.ToString() != "https://localhost:7023/Account/Register")
        {
            return RedirectToAction("Register");
        }


        var vm = new SecuirtyCodeVM()
        {
            Email = TempData["Email"]?.ToString() ?? "",
            Phone = TempData["Phone"]?.ToString() ?? ""
        };

        return View(vm);
    }

    // POST: Account/Active
    [HttpPost]
    public IActionResult Active(SecuirtyCodeVM vm)
    {
        var student = db.Students.FirstOrDefault(c => c.Email == vm.Email && c.SecurityCode == vm.SecurityCode);
        var lecture = db.Lecturers.FirstOrDefault(c => c.Email == vm.Email && c.SecurityCode == vm.SecurityCode);

        if (student == null)
        {
            // Not match
            ModelState.AddModelError("", "Security Code Not Matched.");
        }
        else
        {
            // Match
            student.Status = true;
            db.SaveChanges();
            TempData["Info"] = "Account Activated.";
            return RedirectToAction("Login");
        }

        if (lecture == null)
        {
            // Not match
            ModelState.AddModelError("", "Security Code Not Matched.");
        }
        else
        {
            // Match
            lecture.Status = true;
            db.SaveChanges();
            TempData["Info"] = "Account Activated.";
            return RedirectToAction("Login");
        }


        // TODO: Twilio - Setup
        //var sid = cf["Twilio:SID"];
        //var token = cf["Twilio:Token"];
        //var phone = cf["Twilio:Phone"];

        //TwilioClient.Init(sid, token);

        //// TODO: Twilio - Send SMS
        //try
        //{
        //    var message = MessageResource.Create(
        //        from: new PhoneNumber(phone),
        //        to: new PhoneNumber(vm.Phone),
        //        body: vm.SecurityCode
        //        );
        //    TempData["Info"] = "Register Successfully.";
        //    return RedirectToAction("Login");
        //}
        //catch (TwilioException ex)
        //{
        //    TempData["Info"] = $"ERROR:{ex.Message}";
        //}


        return View(vm);
    }


    // GET: Home/Maintenance - AJAX Combined
    public IActionResult Demo11(string? name, string? sort, string? dir, int page = 1)
    {
        // (1) Searching ------------------------
        name ??= "";
        ViewBag.Name = name = name.Trim();

        var searched = db.Users.Where(s => s.Name.Contains(name));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<User, object> fn = sort switch
        {
            "Email" => s => s.Email,
            "Name" => s => s.Name,
            "Gender" => s => s.Gender,
            "Phone Number" => s => s.PhoneNumber, 
            "Addres" => s => s.Address,
            _ => s => s.Email,
        };

        var sorted = dir == "des" ?
                     searched.OrderByDescending(fn) :
                     searched.OrderBy(fn);

        // (3) Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, sort, dir, page = 1 });
        }

        var model = sorted.ToPagedList(page, 10);

        if (page > model.PageCount && model.PageCount > 0)
        {
            return RedirectToAction(null, new { name, sort, dir, page = model.PageCount });
        }

        if (Request.IsAjax())
        {
            return PartialView("_Maintenance", model);
        }

        return View(model);
    }

    // GET: Account/UpdateProfile
    [Authorize]
    public IActionResult UpdateProfile()
    {
        var s = db.Users.Find(User.Identity!.Name);
        if (s == null) return RedirectToAction("Index", "Home");

        var vm = new UpdateProfileVM
        {
            Email = s.Email,
            Name = s.Name,
            Phone = s.PhoneNumber,
            Address = s.Address,
            PhotoURL = s.PhotoURL

        };

        return View(vm);
    }

    // POST: Account/UpdateProfile
    [Authorize]
    [HttpPost]
    public IActionResult UpdateProfile(UpdateProfileVM vm)
    {
        var s = db.Users.Find(User.Identity!.Name);
        if (s == null) return RedirectToAction("Index", "Home");

        if (vm.Photo != null)
        {
            var err = hp.ValidatePhoto(vm.Photo);
            if (err != "") ModelState.AddModelError("Photo", err);
        }

        if (ModelState.IsValid)
        {
            s.Name = vm.Name;
            s.PhoneNumber = vm.Phone;
            s.Address = vm.Address;

            if (vm.Photo != null)
            {
                hp.DeletePhoto(s.PhotoURL);
                s.PhotoURL = hp.SavePhoto(vm.Photo);
                HttpContext.Session.SetString("PhotoURL", s.PhotoURL);
            }

            db.SaveChanges();

            TempData["Info"] = "Profile updated.";
            return RedirectToAction();
        }

        vm.Email = s.Email;
        vm.PhotoURL = s.PhotoURL;
        return View(vm);
    }

    // GET: Account/UpdatePassword
    [Authorize]
    public IActionResult UpdatePassword()
    {
        return View();
    }

    // POST: Account/UpdatePassword
    [Authorize]
    [HttpPost]
    public IActionResult UpdatePassword(UpdatePasswordVM vm)
    {
        var u = db.Users.Find(User.Identity!.Name);
        if (u == null) return RedirectToAction("Index", "Home");

        if (!hp.VerifyPassword(u.Hash, vm.Current))
        {
            ModelState.AddModelError("Current", "Current Password Not Matched.");
        }

        if (ModelState.IsValid)
        {
            u.Hash = hp.HashPassword(vm.New);
            db.SaveChanges();

            TempData["Info"] = "Password Updated.";
            return RedirectToAction();
        }

        return View();
    }

    // GET: Account/ResetPassword
    public IActionResult ResetPassword()
    {
        return View();
    }

    // POST: Account/ResetPassword
    [HttpPost]
    public IActionResult ResetPassword(ResetPasswordVM vm)
    {
        var u = db.Users.Find(vm.Email);

        if (u == null)
        {
            ModelState.AddModelError("Email", "Email Not Found.");
        }

        if (ModelState.IsValid)
        {
            string password = hp.RandomPassword();

            u!.Hash = hp.HashPassword(password);
            db.SaveChanges();

            SendResetPasswordEmail(u, password);

            TempData["Info"] = "Password Reset. Check Your Email.";
            return RedirectToAction();
        }

        return View();
    }

    private void SendResetPasswordEmail(User u, string password)
    {
        var mail = new MailMessage();
        mail.To.Add(new MailAddress(u.Email, u.Name));
        mail.Subject = "Reset Password";
        mail.IsBodyHtml = true;

        var url = Url.Action("Login", "Account", null, "https");

        var path = u switch
        {
            Admin => Path.Combine(en.WebRootPath, "images", "admin.png"),
            Student s => Path.Combine(en.WebRootPath, "photos", s.PhotoURL),
            Lecturer l => Path.Combine(en.WebRootPath, "photos", l.PhotoURL),
            _ => ""
        };

        var att = new Attachment(path);
        mail.Attachments.Add(att);
        att.ContentId = "photo";

        mail.Body = $@"
        <img src='cid:photo' style='width: 200px; height: 200px; border: 1px solid #333'>
        <p>Dear {u.Name},<p>
        <p>Your password has been reset to:</p>
        <h1 style='color: red'>{password}</h1>
        <p>
            Please <a href='{url}'>login</a>
            with your new password.
        </p>
        <p>From, TarChat Admin 👤</p>
    ";

        hp.SendEmail(mail);
    }

    // GET: Account/Maintenance
    [Authorize]
    public IActionResult Maintenance(string? name, string? sort, string? dir, int page = 1)
    {
        //(1) Searching------------------------
        name ??= "";
        ViewBag.Name = name = name.Trim();

        var searched = db.Users.Where(s => s.Name.Contains(name));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<User, object> fn = sort switch
        {
            "Email" => s => s.Email,
            "Name" => s => s.Name,
            "Gender" => s => s.Gender,
            "Phone Number" => s => s.PhoneNumber,
            "Address" => s => s.Address,
            "Role" => s => s.Role,
            "Faculty" => s => s.Faculty_Code,
            "Programme" => s => s.Programme_Code,
            "Intake Year" => s => s.Year,
            _ => s => s.Email,
        };

        var sorted = dir == "des" ?
                     searched.OrderByDescending(fn) :
                     searched.OrderBy(fn);

        // (3) Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, sort, dir, page = 1 });
        }

        var model = sorted.ToPagedList(page, 10);

        if (page > model.PageCount && model.PageCount > 0)
        {
            return RedirectToAction(null, new { name, sort, dir, page = model.PageCount });
        }

        // TODO: Partial View = _D.cshtml
        if (Request.IsAjax())
        {
            return PartialView("_Maintenance", model);
        }
        return View(model);

    }

    // GET: Account/SuperAdminMaintenance
    [Authorize]
    public IActionResult SuperAdminMaintenance(string? name, string? sort, string? dir, int page = 1)
    {
        //(1) Searching------------------------
        name ??= "";
        ViewBag.Name = name = name.Trim();

        var searched = db.Users.Where(s => s.Name.Contains(name));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<User, object> fn = sort switch
        {
            "Email" => s => s.Email,
            "Name" => s => s.Name,
            "Gender" => s => s.Gender,
            "Phone Number" => s => s.PhoneNumber,
            "Address" => s => s.Address,
            "Role" => s => s.Role,
            "Faculty" => s => s.Faculty_Code,
            "Programme" => s => s.Programme_Code,
            "Intake Year" => s => s.Year,
            _ => s => s.Email,
        };

        var sorted = dir == "des" ?
                     searched.OrderByDescending(fn) :
                     searched.OrderBy(fn);

        // (3) Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, sort, dir, page = 1 });
        }

        var model = sorted.ToPagedList(page, 10);

        if (page > model.PageCount && model.PageCount > 0)
        {
            return RedirectToAction(null, new { name, sort, dir, page = model.PageCount });
        }

        // TODO: Partial View = _D.cshtml
        if (Request.IsAjax())
        {
            return PartialView("_SuperAdminMaintenance", model);
        }
        return View(model);

    }

    // GET: Account/Detail
    public IActionResult Detail(string email)
    {
        var model = db.Users.Find(email);

        if (model == null)
        {
            return RedirectToAction("Index");
        }

        return View(model);
    }

    // GET: Account/DeleteUser
    [Authorize(Roles = "SA")]
    public IActionResult DeleteUser()
    {
        var model = db.Users;
        return View(model);
    }

    // POST: Account/Delete
    [HttpPost]
    public IActionResult Delete(string? email)
    {
        var s = db.Users.Find(email);

        if (s != null)
        {
            db.Users.Remove(s);
            db.SaveChanges();

            if (!Request.IsAjax())
            {
                TempData["Info"] = "Record Deleted.";
            }
        }

        return Redirect(Request.Headers.Referer.ToString());
    }

    // POST: Account/DeleteMany
    [HttpPost]
    public IActionResult DeleteMany(string[] ids)
    {
        var list = db.Users.Where(s => ids.Contains(s.Email));
        db.Users.RemoveRange(list);
        var n = db.SaveChanges();
        TempData["Info"] = $"{n} record(s) Deleted.";
        return RedirectToAction("DeleteUser");
    }

    
    // GET: Account/EditProfile
    public IActionResult EditProfile(string? email)
    {
        var s = db.Users.Find(email); //PK

        if (s == null)
        {
            return RedirectToAction("Index");
        }

        var vm = new EditProfileVM
        {
            Email = s.Email,
            Name = s.Name,
            //Gender = s.Gender,
            Phone = s.PhoneNumber,
            Address = s.Address,
            Faculty = s.Faculty_Code,
            Programme = s.Programme_Code,
            IntakeYear = s.Year,
            Role = s.Role 
        };

        return View(vm);
    }

    // POST: Account/EditProfile
    [HttpPost]
    public IActionResult EditProfile(EditProfileVM vm)
    {
        var s = db.Users.Find(vm.Email); // PK

        if (s == null)
        {
            return RedirectToAction("Index");
        }

        if (ModelState.IsValid("Email")
            && !db.Users.Any(s => s.Email == vm.Email))
        {
            ModelState.AddModelError("Email", "Invalid Email.");
        }

        if (!ModelState.IsValid)
        {
            s.Name = vm.Name;
            //s.Gender = vm.Gender;
            s.PhoneNumber = vm.Phone;
            s.Address = vm.Address;
            s.Faculty_Code = vm.Faculty;
            s.Programme_Code = vm.Programme;
            s.Year = vm.IntakeYear;
            
            db.SaveChanges();


            TempData["Info"] = "Record updated.";
            //return RedirectToAction("Index");
            // Check user's role and redirect accordingly
            if (User.IsInRole("SA"))
            {
                return RedirectToAction("SuperAdminMaintenance", "Account");
            }
            else
            {
                return RedirectToAction("Maintenance", "Account");
            }
        }
        else
        {
            // Check user's role and redirect accordingly
            if (User.IsInRole("SA"))
            {
                return RedirectToAction("SuperAdminMaintenance", "Account");
            }
            else
            {
                return RedirectToAction("Maintenance", "Account");
            }
        }
    }

    // GET: Account/Insert
    [Authorize]
    public IActionResult Insert()
    {
        return View();
    }

    // POST: Account/Insert
    [HttpPost]
    public IActionResult Insert(InsertVM vm)
    {
        if (ModelState.IsValid("Email") && db.Users.Any(u => u.Email == vm.Email))
        {
            ModelState.AddModelError("Email", "Duplicated Email.");
        }

        if (ModelState.IsValid("Photo"))
        {
            var err = hp.ValidatePhoto(vm.Photo);
            if (err != "") ModelState.AddModelError("Photo", err);
        }

        if (ModelState.IsValid)
        {
            var code = Helper.RandomString(6);

            var newUser = new User
            {
                Email = vm.Email,
                Name = vm.Name,
                Gender = vm.Gender,
                PhoneNumber = vm.Phone,
                Address = vm.Address,
                Year = vm.IntakeYear,
                Faculty_Code = vm.Faculty,
                Programme_Code = vm.Programme,
                Hash = hp.HashPassword(vm.Password),
                PhotoURL = hp.SavePhoto(vm.Photo),
                Status = false,
                SecurityCode = code,
                CreationDate = DateTime.Now,

                Role = vm.Role // Set the role based on user selection

            };

            if (vm.Role == "S")
            {
                var newStudent = new Student
                {
                    Email = newUser.Email,
                    Name = newUser.Name,
                    Gender = newUser.Gender,
                    PhoneNumber = newUser.PhoneNumber,
                    Address = newUser.Address,
                    Year = newUser.Year,
                    Faculty_Code = newUser.Faculty_Code,
                    Programme_Code = newUser.Programme_Code,
                    Hash = newUser.Hash,
                    Role = newUser.Role,
                    PhotoURL = hp.SavePhoto(vm.Photo),
                    Status = false,
                    SecurityCode = code,
                    CreationDate = DateTime.Now,

                };
                db.Students.Add(newStudent);

            }
            else if (vm.Role == "L")
            {
                var newLecturer = new Lecturer

                {
                    Email = newUser.Email,
                    Name = newUser.Name,
                    Gender = newUser.Gender,
                    PhoneNumber = newUser.PhoneNumber,
                    Address = newUser.Address,
                    Hash = newUser.Hash,
                    Role = newUser.Role,
                    PhotoURL = hp.SavePhoto(vm.Photo),
                    Status = false,
                    SecurityCode = code,
                    CreationDate = DateTime.Now,

                };
                db.Lecturers.Add(newLecturer);
            }
            else if (vm.Role == "A")
            {
                var newAdmin = new Admin

                {
                    Email = newUser.Email,
                    Name = newUser.Name,
                    Gender = newUser.Gender,
                    PhoneNumber = newUser.PhoneNumber,
                    Address = newUser.Address,
                    Hash = newUser.Hash,
                    Role = newUser.Role,
                    PhotoURL = hp.SavePhoto(vm.Photo),
                    Status = false,
                    SecurityCode = code,
                    CreationDate = DateTime.Now,

                };
                db.Admins.Add(newAdmin);
            }

            db.SaveChanges();

            TempData["Info"] = "Insert Successfully.";
            // Redirect to different pages based on user role
            if (User.IsInRole("SA"))
            {
                return RedirectToAction("SuperAdminMaintenance", "Account");
            }
            else
            {
                return RedirectToAction("Maintenance", "Account");
            }
        }

        return View();
    }

    // GET: Account/Captcha

    public IActionResult Captcha()
    {
        return View();
    }
}

