﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using DNTCaptcha.Core;
using FireSharp.Config;
using FireSharp.Interfaces;

namespace Demo;

public class Helper
{
    private readonly IWebHostEnvironment en;
    private readonly IHttpContextAccessor ct;
    private readonly DB db;
    private readonly IConfiguration cf;

    public Helper(IWebHostEnvironment en, IHttpContextAccessor ct, DB db, IConfiguration cf)
    {
        this.en = en;
        this.ct = ct;
        this.db = db;
        this.cf = cf;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddDNTCaptcha(option => option.UseCookieStorageProvider()
        .ShowThousandsSeparators(false));
        services.AddSignalR();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();  // For API controllers
            endpoints.MapRazorPages();   // If you're using Razor Pages
            endpoints.MapHub<ScreenShareHub>("/screenShareHub");
        });
    }


    // ------------------------------------------------------------------------
    // Photo Upload Helper Functions
    // ------------------------------------------------------------------------

    //private const string UPLOAD_FOLDER = "products";
    private const string UPLOAD_FOLDER = "photos";

    public string ValidatePhoto(IFormFile f)
    {
        var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
        var reName = new Regex(@"^.+\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase);

        if (!reType.IsMatch(f.ContentType) || !reName.IsMatch(f.FileName))
        {
            return "Only JPG or PNG photo is allowed.";
        }
        else if (f.Length > 1 * 1024 * 1024)
        {
            return "Photo size cannot more than 1MB.";
        }

        return "";
    }

    public string SavePhoto(IFormFile f)
    {
        var file = Guid.NewGuid().ToString("n") + ".jpg";
        var path = Path.Combine(en.WebRootPath, UPLOAD_FOLDER, file);

        var options = new ResizeOptions
        {
            Size = new(200, 200),
            Mode = ResizeMode.Crop
        };

        using var stream = f.OpenReadStream();
        using var img = Image.Load(stream);
        img.Mutate(img => img.Resize(options));
        img.Save(path);

        return file;
    }

    public void DeletePhoto(string file)
    {
        file = Path.GetFileName(file);
        var path = Path.Combine(en.WebRootPath, UPLOAD_FOLDER, file);
        File.Delete(path);
    }



    // ------------------------------------------------------------------------
    // Security Helper Functions
    // ------------------------------------------------------------------------

    private readonly PasswordHasher<object> ph = new();

    public string HashPassword(string password)
    {
        return ph.HashPassword(0, password);
    }

    public bool VerifyPassword(string hash, string password)
    {
        return ph.VerifyHashedPassword(0, hash, password) == PasswordVerificationResult.Success;
    }

    public void SignIn(string email, string role, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = rememberMe
        };

        ct.HttpContext?.SignInAsync(principal, properties);
    }

    public void SignOut()
    {
        ct.HttpContext?.SignOutAsync();
    }

    public string RandomPassword()
    {
        string s = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string password = "";

        Random r = new();

        for (int i = 1; i <= 10; i++)
        {
            password += s[r.Next(s.Length)];
        }

        return password;
    }

    
    public string? GetUserPhotoURL()
    {
        var photoURL = ct.HttpContext?.Session.GetString("PhotoURL");

        if (photoURL == null)
        {
            photoURL = db.Users
                         .Find(ct.HttpContext?.User.Identity?.Name)?
                         .PhotoURL;

            if (photoURL != null)
            {
                ct.HttpContext?.Session.SetString("PhotoURL", photoURL);
            }
        }

        return photoURL;
    }


    // ------------------------------------------------------------------------
    // Email Helper Functions
    // ------------------------------------------------------------------------

    public void SendEmail(MailMessage mail)
    {
        string user = cf["Smtp:User"] ?? "";
        string pass = cf["Smtp:Pass"] ?? "";
        string name = cf["Smtp:Name"] ?? "";
        string host = cf["Smtp:Host"] ?? "";
        int port = cf.GetValue<int>("Smtp:Port");

        mail.From = new MailAddress(user, name);

        var smtp = new SmtpClient
        {
            Host = host,
            Port = port,
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass)
        };
        
        smtp.Send(mail);
    }

    // ------------------------------------------------------------------------
    // Random Helper Functions
    // ------------------------------------------------------------------------

    public static string RandomString(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // ------------------------------------------------------------------------
    // DateTime Helper Functions
    // ------------------------------------------------------------------------

    // Return select list for years
    public SelectList GetYearList(int min, int max, bool reverse = false)
    {
        var items = new List<int>();
        for (int n = min; n <= max; n++)
        {
            items.Add(n);
        }
        if (reverse) items.Reverse();
        return new SelectList(items);
    }

    // Given month id (1-12), return month abbrevation (Jan-Dec)
    public string GetMonthAbbr(int n)
    {
        return new DateTime(1, n, 1).ToString("MMM");
    }

    // Return select list for months (id and name)
    public SelectList GetMonthList()
    {
        var items = new List<object>();
        for (int n = 1; n <= 12; n++)
        {
            items.Add(new { Id = n, Name = new DateTime(1, n, 1).ToString("MMMM") });
        }
        return new SelectList(items, "Id", "Name");
    }   
}
