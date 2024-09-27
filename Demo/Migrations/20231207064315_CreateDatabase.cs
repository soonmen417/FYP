using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Facultys",
                columns: table => new
                {
                    FacultyCode = table.Column<string>(name: "Faculty_Code", type: "nvarchar(4)", maxLength: 4, nullable: false),
                    FacultyName = table.Column<string>(name: "Faculty_Name", type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facultys", x => x.FacultyCode);
                });

            migrationBuilder.CreateTable(
                name: "IntakeYears",
                columns: table => new
                {
                    Year = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeYears", x => x.Year);
                });

            migrationBuilder.CreateTable(
                name: "Programmes",
                columns: table => new
                {
                    ProgrammeCode = table.Column<string>(name: "Programme_Code", type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ProgrammeName = table.Column<string>(name: "Programme_Name", type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FacultyCode = table.Column<string>(name: "Faculty_Code", type: "nvarchar(4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programmes", x => x.ProgrammeCode);
                    table.ForeignKey(
                        name: "FK_Programmes_Facultys_Faculty_Code",
                        column: x => x.FacultyCode,
                        principalTable: "Facultys",
                        principalColumn: "Faculty_Code");
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomName = table.Column<string>(name: "Room_Name", type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RoomDescription = table.Column<string>(name: "Room_Description", type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoomType = table.Column<string>(name: "Room_Type", type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RoomCreatedTime = table.Column<DateTime>(name: "Room_CreatedTime", type: "datetime2", maxLength: 50, nullable: false),
                    FacultyCode = table.Column<string>(name: "Faculty_Code", type: "nvarchar(4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Rooms_Facultys_Faculty_Code",
                        column: x => x.FacultyCode,
                        principalTable: "Facultys",
                        principalColumn: "Faculty_Code");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhotoURL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "bit", maxLength: 10, nullable: false),
                    SecurityCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", maxLength: 50, nullable: false),
                    Year = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    FacultyCode = table.Column<string>(name: "Faculty_Code", type: "nvarchar(4)", nullable: true),
                    ProgrammeCode = table.Column<string>(name: "Programme_Code", type: "nvarchar(4)", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Email);
                    table.ForeignKey(
                        name: "FK_Users_Facultys_Faculty_Code",
                        column: x => x.FacultyCode,
                        principalTable: "Facultys",
                        principalColumn: "Faculty_Code");
                    table.ForeignKey(
                        name: "FK_Users_IntakeYears_Year",
                        column: x => x.Year,
                        principalTable: "IntakeYears",
                        principalColumn: "Year");
                    table.ForeignKey(
                        name: "FK_Users_Programmes_Programme_Code",
                        column: x => x.ProgrammeCode,
                        principalTable: "Programmes",
                        principalColumn: "Programme_Code");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "DATE", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLists",
                columns: table => new
                {
                    UserListID = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLists", x => x.UserListID);
                    table.ForeignKey(
                        name: "FK_UserLists_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLists_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RoomId",
                table: "Messages",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserEmail",
                table: "Messages",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Programmes_Faculty_Code",
                table: "Programmes",
                column: "Faculty_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Faculty_Code",
                table: "Rooms",
                column: "Faculty_Code");

            migrationBuilder.CreateIndex(
                name: "IX_UserLists_RoomId",
                table: "UserLists",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLists_UserEmail",
                table: "UserLists",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Faculty_Code",
                table: "Users",
                column: "Faculty_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Programme_Code",
                table: "Users",
                column: "Programme_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Year",
                table: "Users",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "UserLists");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "IntakeYears");

            migrationBuilder.DropTable(
                name: "Programmes");

            migrationBuilder.DropTable(
                name: "Facultys");
        }
    }
}
