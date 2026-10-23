using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartShop.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Features = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Sentiment = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.ReviewId);
                    table.CheckConstraint("CK_Review_Rating", "[Rating] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Review_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "ProductId", "Category", "Description", "Features", "Price", "ProductName" },
                values: new object[,]
                {
                    { 1, "Phone", null, "6.2 inch AMOLED, Snapdragon 8 Elite, 50MP camera", 22990000m, "Galaxy S25" },
                    { 2, "Laptop", null, "13.6 inch, 16GB RAM, 18 hours battery", 28490000m, "MacBook Air M4" },
                    { 3, "Accessory", null, "Active noise cancellation, USB-C, IP57", 6790000m, "AirPods Pro 3" },
                    { 4, "Phone", null, "120Hz display, 5500mAh battery, 108MP camera", 6490000m, "Xiaomi Redmi Note 15" }
                });

            migrationBuilder.InsertData(
                table: "Review",
                columns: new[] { "ReviewId", "Content", "CreatedAt", "CustomerName", "ProductId", "Rating", "Score", "Sentiment", "Summary" },
                values: new object[,]
                {
                    { 1, "Screen is gorgeous and the phone is very fast.", new DateTime(2026, 1, 2, 9, 0, 0, 0, DateTimeKind.Unspecified), "Nguyen An", 1, 5, 0.94999999999999996, "Positive", "Gorgeous screen and very fast performance." },
                    { 2, "Camera takes amazing photos even at night.", new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), "Tran Binh", 1, 5, 0.93000000000000005, "Positive", "Excellent camera, great night photos." },
                    { 3, "Great phone overall, battery could be better.", new DateTime(2026, 1, 4, 9, 0, 0, 0, DateTimeKind.Unspecified), "Le Chi", 1, 5, 0.81000000000000005, "Positive", "Great phone, battery could be better." },
                    { 4, "Battery is great but the phone gets hot when gaming.", new DateTime(2026, 1, 5, 9, 0, 0, 0, DateTimeKind.Unspecified), "Pham Dung", 1, 2, 0.71999999999999997, "Negative", "Phone overheats when gaming." },
                    { 5, "It is okay, nothing special for the price.", new DateTime(2026, 1, 6, 9, 0, 0, 0, DateTimeKind.Unspecified), "Hoang Em", 1, 3, 0.66000000000000003, "Neutral", "Average phone for its price." },
                    { 6, "Battery lasts all day and it is super light.", new DateTime(2026, 1, 7, 9, 0, 0, 0, DateTimeKind.Unspecified), "Vo Giang", 2, 5, 0.95999999999999996, "Positive", "All-day battery and very lightweight." },
                    { 7, "Silent, fast and the display is beautiful.", new DateTime(2026, 1, 8, 9, 0, 0, 0, DateTimeKind.Unspecified), "Dang Hai", 2, 5, 0.93999999999999995, "Positive", "Silent, fast laptop with a beautiful display." },
                    { 8, "Very good laptop, only wish it had more ports.", new DateTime(2026, 1, 9, 9, 0, 0, 0, DateTimeKind.Unspecified), "Bui Khanh", 2, 4, 0.83999999999999997, "Positive", "Very good laptop but few ports." },
                    { 9, "Left earbud stopped working after one week.", new DateTime(2026, 1, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), "Do Lan", 3, 1, 0.94999999999999996, "Negative", "Left earbud failed after a week." },
                    { 10, "Noise cancellation is weak and they fall out of my ears.", new DateTime(2026, 1, 11, 9, 0, 0, 0, DateTimeKind.Unspecified), "Ngo Minh", 3, 2, 0.88, "Negative", "Weak noise cancellation and poor fit." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Review_ProductId",
                table: "Review",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "Product");
        }
    }
}
