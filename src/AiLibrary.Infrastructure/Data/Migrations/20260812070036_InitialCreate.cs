using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AiLibrary.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Genre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ReadingLevel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Blurb = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Blurb", "Genre", "PageCount", "ReadingLevel", "Tags", "Title" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), "J.R.R. Tolkien", "A reluctant hobbit joins dwarves on a treasure quest filled with wit and wonder.", "Fantasy", 310, "Middle grade / YA+", "adventureclassicquestdragons", "The Hobbit" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), "Ursula K. Le Guin", "A gifted young mage must face the shadow unleashed by his own pride.", "Fantasy", 183, "YA / Adult", "magiccoming-of-ageclassic", "A Wizard of Earthsea" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), "Jane Austen", "Sharp social comedy about first impressions, pride, and unlikely affection.", "Romance / Classic", 432, "Adult", "romanceclassicwit", "Pride and Prejudice" },
                    { new Guid("11111111-1111-1111-1111-111111111104"), "Agatha Christie", "Hercule Poirot investigates a locked-train murder with too many suspects.", "Mystery", 256, "Adult", "detectiveclassicpuzzle", "Murder on the Orient Express" },
                    { new Guid("11111111-1111-1111-1111-111111111105"), "Frank Herbert", "Political intrigue and destiny collide on a desert planet that holds the universe's most valuable resource.", "Science Fiction", 688, "Adult", "epicpoliticsworldbuilding", "Dune" },
                    { new Guid("11111111-1111-1111-1111-111111111106"), "F. Scott Fitzgerald", "Jazz Age tragedy of longing, wealth, and the American dream.", "Literary Fiction", 180, "Adult / YA+", "classicshorttragedy", "The Great Gatsby" },
                    { new Guid("11111111-1111-1111-1111-111111111107"), "Octavia E. Butler", "A modern woman is pulled through time into the brutal realities of antebellum slavery.", "Science Fiction / Historical", 287, "Adult", "time-travelpowerfulthoughtful", "Kindred" },
                    { new Guid("11111111-1111-1111-1111-111111111108"), "Madeline Miller", "A vivid reimagining of the witch of Aiaia finding power and voice among gods and mortals.", "Mythic Fiction", 393, "Adult", "mythologyfeministlush-prose", "Circe" },
                    { new Guid("11111111-1111-1111-1111-111111111109"), "Andy Weir", "A lone astronaut wakes on a mission to save Earth — with science, humor, and unlikely friendship.", "Science Fiction", 476, "Adult", "spaceproblem-solvingfun", "Project Hail Mary" },
                    { new Guid("11111111-1111-1111-1111-111111111110"), "L.M. Montgomery", "Imaginative orphan Anne Shirley transforms a quiet farm community with spirit and heart.", "Classic / Coming of Age", 320, "Middle grade / YA", "cozyclassicfriendship", "Anne of Green Gables" },
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Erin Morgenstern", "A mysterious circus becomes the stage for a magical competition and a forbidden romance.", "Fantasy", 387, "Adult / YA+", "atmosphericromancemagic", "The Night Circus" },
                    { new Guid("11111111-1111-1111-1111-111111111112"), "Agatha Christie", "Ten strangers invited to an island are killed one by one in a masterclass of suspense.", "Mystery", 272, "Adult", "thrillerclassicisolated", "And Then There Were None" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
