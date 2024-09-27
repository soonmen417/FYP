using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

public class HomeController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public HomeController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
    }

    // GET: Home/Index
    public IActionResult Index()
    {
        return View();
    }

    // GET: Home/AboutUs
    public IActionResult AboutUs()
    {
        return View();
    }

    // GET: Home/OurLocation
    public IActionResult OurLocation()
    {
        return View();
    }

    //// GET: Home/TermsConditions
    //public IActionResult TermsConditions()
    //{
    //    return View();
    //}
    //public IActionResult ChatRoom()
    //{
    //    // TODO: Obtain user name --> ViewBag.Name
    //    ViewBag.Name = db.Users.Find(User.Identity!.Name)?.Name;

    //    return View();
    //}
}
