using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IT15_Project.Migrations
{
    /// <inheritdoc />
    public partial class faresettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "FareSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseFare = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PerKilometerRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PerMinuteRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DriverShareRate = table.Column<int>(type: "int", nullable: false),
                    CommissionRate = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FareSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FareSettings",
                columns: new[] { "Id", "BaseFare", "CommissionRate", "DriverShareRate", "PerKilometerRate", "PerMinuteRate", "SeatType", "VehicleType" },
                values: new object[,]
                {
                    { 1, 50.00m, 20, 80, 15.00m, 2.00m, "4-seater", "Car" },
                    { 2, 50.00m, 20, 80, 18.00m, 2.00m, "6-seater", "Car" },
                    { 3, 30.00m, 10, 90, 5.00m, 1.50m, "1-seater", "Motorcycle" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FareSettings");

           
        }
    }
}
