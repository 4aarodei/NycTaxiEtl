using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NycTaxiEtl.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxiRides",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickupDatetimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DropoffDatetimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PassengerCount = table.Column<byte>(type: "tinyint", nullable: false),
                    TripDistance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StoreAndFwdFlag = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PULocationID = table.Column<short>(type: "smallint", nullable: false),
                    DOLocationID = table.Column<short>(type: "smallint", nullable: false),
                    FareAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TipAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxiRides", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxiRides_Pickup_Dropoff",
                table: "TaxiRides",
                columns: new[] { "PickupDatetimeUtc", "DropoffDatetimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxiRides_PULocationID",
                table: "TaxiRides",
                column: "PULocationID");

            migrationBuilder.CreateIndex(
                name: "IX_TaxiRides_TripDistance_Desc",
                table: "TaxiRides",
                column: "TripDistance",
                descending: new[] { true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxiRides");
        }
    }
}
