using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Twilio.TwiML.Messaging;
using Twilio.TwiML.Voice;

namespace Demo.Models;

public class DB : DbContext
{
    public DB(DbContextOptions<DB> options) : base(options) { }

    // DB Sets
    public DbSet<User> Users { get; set; }
    public DbSet<Lecturer> Lecturers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<SuperAdmin> SuperAdmins { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<UserList> UserLists { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<IntakeYear> IntakeYears { get; set; }
    public DbSet<Faculty> Facultys { get; set; }
    public DbSet<Programme> Programmes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Programme)
            .WithMany(p => p.Users)
            .HasForeignKey(u => u.Programme_Code)
            .HasPrincipalKey(p => p.Programme_Code); // Assuming Programme_Code is the primary key in Programme entity

        modelBuilder.Entity<User>()
            .HasOne(u => u.Faculty)
            .WithMany(f => f.Users)
            .HasForeignKey(u => u.Faculty_Code)
            .HasPrincipalKey(f => f.Faculty_Code); // Assuming Programme_Code is the primary key in Programme entity

        modelBuilder.Entity<User>()
            .HasOne(u => u.IntakeYear)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.Year)
            .HasPrincipalKey(i => i.Year); // Assuming Programme_Code is the primary key in Programme entity
    }
}

// Entity Classes

#nullable disable warnings

public class User
{
    [Key, MaxLength(100)]
    public string Email { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(10)]
    public string Gender { get; set; }

    [MaxLength(20)]
    public string PhoneNumber { get; set; }

    [MaxLength(200)]
    public string Address { get; set; }

    [MaxLength(100)]
    public string Hash { get; set; }

    [MaxLength(10)]
    public string Role { get; set; }

    [MaxLength(100)]
    public string PhotoURL { get; set; }

    [MaxLength(10)]
    public bool Status { get; set; }

    [MaxLength(30)]
    public string? SecurityCode { get; set; }

    [MaxLength(50)]
    public DateTime CreationDate { get; set; }

    // FK
    public string? Year { get; set; }
    public string? Faculty_Code { get; set; }
    public string? Programme_Code { get; set; }

    // Navigation
    public List<UserList> UserLists { get; set; } = new();

    public IntakeYear IntakeYear { get; set; }
    public Faculty Faculty { get; set; }
    public Programme Programme { get; set; }

}

public class Lecturer : User
{

}

public class SuperAdmin : User
{

}

public class Admin : SuperAdmin
{

}

public class Student : User
{

}

// NEW ------------------------------------------------------------------------

public class UserList
{
    // Column
    [Key, MaxLength(15)]
    public string UserListID { get; set; }

    [MaxLength(200)]
    public string Description { get; set; }

    // FK
    //public string StudentEmail { get; set; }
    public string RoomId { get; set; }

    // Navigation
    //public User User { get; set; }
    public Room Room { get; set; }
}

public class Room
{
    // Column
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string RoomId { get; set; }

    [MaxLength(30)]
    public string Room_Name { get; set; }

    [MaxLength(200)]
    public string Room_Description { get; set; }

    [MaxLength(10)]
    public string Room_Type { get; set; }

    [MaxLength(50)]
    public DateTime Room_CreatedTime { get; set; }

    //// FK
    //public string? Faculty_Code { get; set; }


    // Navigation
    public List<UserList> UserLists { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
}

public class Message
{
    // Column
    [Key, MaxLength(15)]
    public string MessageId { get; set; }

    [MaxLength(300)]
    public string Content { get; set; }

    [Column(TypeName = "DATE")]
    public DateTime TimeStamp { get; set; }

    // FK
    public string Email { get; set; }
    public string RoomId { get; set; }

    // Navigation
    public User User { get; set; }
    public Room Room { get; set; }
}

public class IntakeYear
{
    // Column
    //[Key, MaxLength(15)]
    //public string IntakeYearId { get; set; }

    [Key, MaxLength(4)]
    public string? Year { get; set; }

    // Navigation
    public List<User> Users { get; set; } = new();
}

public class Faculty
{
    // Column
    //[Key, MaxLength(15)]
    //public string FacultyId { get; set; }

    [Key, MaxLength(4)]
    public string? Faculty_Code { get; set; }

    [MaxLength(50)]
    public string Faculty_Name { get; set; }

    // Navigation
    public List<User> Users { get; set; } = new();
    public List<Programme> Programmes { get; set; } = new();

    public List<Room> Rooms { get; set; } = new();
}

public class Programme
{
    // Column
    //[Key, MaxLength(15)]
    //public string ProgrammeId { get; set; }

    [Key, MaxLength(4)]
    public string? Programme_Code { get; set; }

    [MaxLength(50)]
    public string Programme_Name { get; set; }

    // FK
    //public string UserId { get; set; }
    //public string FacultyId { get; set; }

    // Navigation
    //public Faculty Faculty { get; set; }
    public List<User> Users { get; set; } = new ();
}


