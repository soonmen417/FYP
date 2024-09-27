using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Linq;
using Demo.Models;
using Azure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Demo.Controllers;


public class ChartController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public ChartController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
    }

    // ------------------------------------------------------------------------
    // Yuan Meng
    // ------------------------------------------------------------------------

    // GET: Chart/GenderReport
    public IActionResult GenderReport()
    {

        return View();
    }

    // GET: Chart/GenderInfo
    public IActionResult GenderInfo()
    {
        // Return User count by gender --> JSON
        var dt = db.Users
                   .GroupBy(s => s.Gender)
                   .Select(g => new object[]
                   {
                       g.Key == "f" ? "Female" : "Male",
                       g.Count()
                   });


        return Json(dt);
    }

    // GET: Chart/RegisterMonth
    public IActionResult RegistrationReport()
    {
        // Return available years for DropDownList
        int min = DateTime.Today.Year;
        int max = DateTime.Today.Year;

        if (db.Users.Count() > 0)
        {
            min = db.Users.Min(o => o.CreationDate).Year;
            max = db.Users.Max(o => o.CreationDate).Year;
        }

        ViewBag.YearList = hp.GetYearList(min, max);

        return View();
    }

    // GET: Chart/RegisterInfo
    public IActionResult RegisterInfo(string role)
    {
        var dict = Enumerable.Range(1, 12).ToDictionary(n => n, _ => 0m);

        // Return overall number of customer Register by month (filter by year) --> JSON
        db.Users
                   .Where(u => u.Role == role)
                   .GroupBy(u => u.CreationDate.Month)
                   .OrderBy(g => g.Key)
                   .Select(g => new
                   {
                       Month = g.Key,
                       User = g.Count()
                   })
                   .ToList() // DB result
                   .ForEach(x => dict[x.Month] = x.User);

        var dt = dict.Select(i => new object[]
        {
            hp.GetMonthAbbr(i.Key),
            i.Value
        }); ;

        return Json(dt);
    }

}

