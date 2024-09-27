using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using System.Net;
using Twilio.TwiML.Messaging;
using Demo.Models;
using FireSharp;
using static System.Net.WebRequestMethods;
using System.Diagnostics;

namespace Demo.Controllers;

public class ChatController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;
    private readonly IConfiguration cf;

    public ChatController(DB db, IWebHostEnvironment en, Helper hp, IConfiguration cf)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
        this.cf = cf;
    }

    IFirebaseConfig config = new FirebaseConfig
    {
        AuthSecret = "ajsxlbV3nisrASyeuznCIzcwnJU3OrG2DLKoEEkv",
        BasePath = "https://soon-0417-default-rtdb.asia-southeast1.firebasedatabase.app/"

    };
    IFirebaseClient client;

    // GET: Chat
    [HttpGet]
    public IActionResult Chat()
    {
        var s = db.Users.Find(User.Identity!.Name);
        if (s == null) return RedirectToAction("Index", "Home");

        var vm = new ChatVM
        {
            PhotoURL = s.PhotoURL,
            Name = s.Name,
        };

        // Get the current user's email from the HttpContext
        var userEmail = User.Identity.Name;

        // Fetch the user based on the role
        var user = db.Users.FirstOrDefault(u => u.Email == userEmail);

        ViewBag.FacultyCode = user.Faculty_Code;
        ViewBag.CurrentUserName = s.Name;

        return View(vm);

    }

    [HttpPost]
    public IActionResult Chat(ChatVM vm)
    {
        try
        {
            client = new FireSharp.FirebaseClient(config);
            var data = vm;

            PushResponse response = client.Push("Private Chats/", data);
            data.Id = response.Result.name;
            SetResponse setResponse = client.Set(" Private Chats/" + data.Id, data);

            if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ModelState.AddModelError(string.Empty, "Added Succesfully");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Something Went Wrong!!");
            }
        }
        catch (Exception ex)
        {

            ModelState.AddModelError(string.Empty, ex.Message);
        }


        return View(vm);
    }

    public IActionResult GetChats()
    {
        try
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Private Chats/");

            var chats = response.ResultAs<Dictionary<string, ChatVM>>();
            List<ChatVM> chatList = chats.Values.ToList();


            return View(chatList);

        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View();
    }

    // GET: Chat
    [HttpGet]
    public IActionResult ChatRoom()
    {
        var s = db.Users.Find(User.Identity!.Name);
        if (s == null) return RedirectToAction("Index", "Home");

        var vm = new ChatVM
        {
            PhotoURL = s.PhotoURL,
            Name = s.Name,
        };

        ViewBag.CurrentUserName = s.Name;

        return View(vm);
    }

    [HttpPost]
    public IActionResult ChatRoom(ChatVM vm)
    {
        try
        {
            client = new FireSharp.FirebaseClient(config);
            var data = vm;

            PushResponse response = client.Push("Public Chats/", data);
            data.Id = response.Result.name;
            SetResponse setResponse = client.Set("Public Chats/" + data.Id, data);

            if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ModelState.AddModelError(string.Empty, "Added Succesfully");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Something Went Wrong!!");
            }
        }
        catch (Exception ex)
        {

            ModelState.AddModelError(string.Empty, ex.Message);
        }


        return View(vm);
    }

    public IActionResult GetChatRooms()
    {
        try
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Public Chats/");

            var chats = response.ResultAs<Dictionary<string, ChatVM>>();
            List<ChatVM> chatList = chats.Values.ToList();


            return View(chatList);

        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View();
    }

    // GET: Chat/EnterChat
    public IActionResult EnterChat()
    {
        return View();
    }

    // POST: Chat/EnterChat
    [HttpPost]
    public IActionResult EnterChat(EnterChatVM vm)
    {
        //var u = db.Rooms.Find(vm.RoomId);
        var room = db.Rooms.FirstOrDefault(r => r.RoomId == vm.RoomId);

        if (room == null)
        {
            ModelState.AddModelError("RoomId", "Room ID Not Found.");
            return View();
        }

        // Get the current user's email from the HttpContext
        var userEmail = User.Identity.Name;

        // Fetch the user based on the role
        var user = db.Users.FirstOrDefault(u => u.Email == userEmail);

        if (user != null)
        {
            // Check if the user's role is Student and Faculty_Code matches the Room_Name
            if (user is Student && user.Faculty_Code == room.Room_Name)
            {
                // Match, user is authorized
                TempData["Info"] = $"Welcome, {user.Name}!";
                return RedirectToAction("Chat");
            }

            //// Check if the user's role is Lecturer
            //if (user is Lecturer)
            //{
            //    // Match, user is authorized
            //    TempData["Info"] = $"Welcome, {user.Name}!";
            //    return RedirectToAction("Chat");
            //}

            // User is not authorized
            ModelState.AddModelError(string.Empty, "You are not authorized to access this chat room.");
        }
        else
        {
            // User not found
            ModelState.AddModelError(string.Empty, "User not found.");
        }

        //if (ModelState.IsValid)
        //{
        //    string password = hp.RandomPassword();

        //    u!.Hash = hp.HashPassword(password);
        //    db.SaveChanges();

        //    SendResetPasswordEmail(u, password);

        //    TempData["Info"] = "Password Reset. Check Your Email.";
        //    return RedirectToAction();
        //}

        return View();
    }

}
