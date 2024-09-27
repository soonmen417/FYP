using Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Demo.Controllers;

public class RoomController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;
    private readonly IConfiguration cf;


    public RoomController(DB db, IWebHostEnvironment en, Helper hp, IConfiguration cf)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
        this.cf = cf;
    }

    // GET: Room/RoomMaintenance
    [Authorize]
    public IActionResult RoomMaintenance(string? name, string? sort, string? dir, int page = 1)
    {
        //(1) Searching------------------------
        name ??= "";
        ViewBag.Name = name = name.Trim();

        var searched = db.Rooms.Where(r => r.Room_Name.Contains(name));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<Room, object> fn = sort switch
        {
            "Room ID" => r => r.RoomId,
            "Room Name" => r => r.Room_Name,
            "Room Description" => r => r.Room_Description,
            "Room Type" => r => r.Room_Type,
            _ => r => r.RoomId,
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
            return PartialView("_RoomMaintenance", model);
        }
        return View(model);

    }

    // GET: Room/CheckRoomId
    public bool CheckRoomId(string roomid)
    {
        return !db.Rooms.Any(r => r.RoomId == roomid);
    }

    private string NextId()
    {
        string max = db.Rooms.Max(r => r.RoomId) ?? "R000";
        int n = int.Parse(max[1..]);
        return (n + 1).ToString("'R'000");
    }

    // GET: Room/RoomInsert
    [Authorize]
    public IActionResult RoomInsert()
    {
        var vm = new RoomInsertVM
        {
            RoomId = NextId()
        };

        return View(vm);
    }

    // POST: Room/RoomInsert
    [HttpPost]
    public IActionResult RoomInsert(RoomInsertVM vm)
    {
        if (ModelState.IsValid("RoomId") && db.Rooms.Any(r => r.RoomId == vm.RoomId))
        {
            ModelState.AddModelError("RoomId", "Duplicated Room Id.");
        }

        //if (ModelState.IsValid("Photo"))
        //{
        //    var err = hp.ValidatePhoto(vm.Photo);
        //    if (err != "") ModelState.AddModelError("Photo", err);
        //}

        if (ModelState.IsValid)
        {
            db.Rooms.Add(new()
            {
                RoomId = vm.RoomId,
                Room_Name = vm.Room_Name,
                //Date = vm.Date,
                Room_Description = vm.Room_Description,
                Room_Type = vm.Room_Type,
                Room_CreatedTime = DateTime.Now,
            });
            db.SaveChanges();

            TempData["Info"] = "Insert Successfully.";
            return RedirectToAction("RoomMaintenance");
        }

        return View();
    }

    // GET: Room/RoomDetail
    public IActionResult RoomDetail(string roomid)
    {
        var model = db.Rooms.Find(roomid);

        if (model == null)
        {
            return RedirectToAction("Index");
        }

        return View(model);
    }

    // GET: Room/EditRoom
    public IActionResult EditRoom(string? roomid)
    {
        var r = db.Rooms.Find(roomid); //PK

        if (r == null)
        {
            return RedirectToAction("Index");
        }

        var vm = new EditRoomVM
        {
            RoomId = r.RoomId,
            Room_Name = r.Room_Name,
            Room_Description = r.Room_Description,
            Room_Type = r.Room_Type,
        };

        return View(vm);
    }

    // POST: Room/EditRoom
    [HttpPost]
    public IActionResult EditRoom(EditRoomVM vm)
    {
        var r = db.Rooms.Find(vm.RoomId); // PK

        if (r == null)
        {
            return RedirectToAction("Index");
        }

        if (ModelState.IsValid)
        {
            r.RoomId = vm.RoomId;
            r.Room_Name = vm.Room_Name;
            r.Room_Description = vm.Room_Description;
            r.Room_Type = vm.Room_Type;
            db.SaveChanges();

            TempData["Info"] = "Record Updated.";
            //return RedirectToAction("Index");
            return RedirectToAction("RoomMaintenance", "Room");
        }
        else
        {
            return RedirectToAction("RoomMaintenance", "Room");
        }


    }

    // GET: Room/DeleteRoom
    [Authorize(Roles = "SA")]
    public IActionResult DeleteRoom()
    {
        var model = db.Rooms;
        return View(model);
    }

    // POST: Room/Delete
    [HttpPost]
    public IActionResult Delete(string? roomid)
    {
        var r = db.Rooms.Find(roomid);

        if (r != null)
        {
            db.Rooms.Remove(r);
            db.SaveChanges();

            if (!Request.IsAjax())
            {
                TempData["Info"] = "Record Deleted.";
            }
        }

        return Redirect(Request.Headers.Referer.ToString());
    }

    // POST: Room/DeleteMany
    [HttpPost]
    public IActionResult DeleteMany(string[] ids)
    {
        var list = db.Rooms.Where(r => ids.Contains(r.RoomId));
        db.Rooms.RemoveRange(list);
        var n = db.SaveChanges();
        TempData["Info"] = $"{n} record(s) Deleted.";
        return RedirectToAction("DeleteRoom");
    }
}

