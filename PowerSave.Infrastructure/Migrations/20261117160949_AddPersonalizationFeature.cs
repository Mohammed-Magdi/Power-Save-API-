using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerSave.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalizationFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplianceCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryType = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplianceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HouseholdSize = table.Column<int>(type: "int", nullable: true),
                    HomeType = table.Column<int>(type: "int", nullable: true),
                    HomeArea = table.Column<int>(type: "int", nullable: true),
                    NumberOfRooms = table.Column<int>(type: "int", nullable: true),
                    DaytimePresence = table.Column<int>(type: "int", nullable: true),
                    IsSkipped = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HouseholdProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LifestyleProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyRoutine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HomeTimeSlotsCsv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WakeUpTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    BedTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    WeekendRoutine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsSkipped = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifestyleProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LifestyleProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeterSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeterType = table.Column<int>(type: "int", nullable: false),
                    BillPaymentDay = table.Column<int>(type: "int", nullable: false),
                    ConsumptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonthlyBillAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MonthlyCardBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsSkipped = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterSetups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterSetups_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalizationProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    StageStatus = table.Column<int>(type: "int", nullable: false),
                    CompletedStagesCsv = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SkippedStagesCsv = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalizationProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalizationProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingTargets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavingPercentage = table.Column<int>(type: "int", nullable: true),
                    IsSkipped = table.Column<bool>(type: "bit", nullable: false),
                    ConfiguredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingTargets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplianceCatalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsModelSensitive = table.Column<bool>(type: "bit", nullable: false),
                    DefaultWattage = table.Column<double>(type: "float", nullable: false),
                    DefaultDailyHours = table.Column<double>(type: "float", nullable: false),
                    MaxAllowedQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplianceCatalogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplianceCatalogs_ApplianceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ApplianceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplianceModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplianceCatalogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ModelNameOrNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CapacityOrSize = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnergyRating = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Wattage = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplianceModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplianceModels_ApplianceCatalogs_ApplianceCatalogId",
                        column: x => x.ApplianceCatalogId,
                        principalTable: "ApplianceCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAppliances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplianceCatalogId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplianceModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsModelUnknown = table.Column<bool>(type: "bit", nullable: false),
                    IsModelSkipped = table.Column<bool>(type: "bit", nullable: false),
                    Wattage = table.Column<double>(type: "float", nullable: false),
                    IsWattageEstimated = table.Column<bool>(type: "bit", nullable: false),
                    DailyOperatingHours = table.Column<double>(type: "float", nullable: false),
                    RoomLocation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAppliances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAppliances_ApplianceCatalogs_ApplianceCatalogId",
                        column: x => x.ApplianceCatalogId,
                        principalTable: "ApplianceCatalogs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserAppliances_ApplianceModels_ApplianceModelId",
                        column: x => x.ApplianceModelId,
                        principalTable: "ApplianceModels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserAppliances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplianceCatalogs_CategoryId",
                table: "ApplianceCatalogs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplianceCategories_CategoryType",
                table: "ApplianceCategories",
                column: "CategoryType");

            migrationBuilder.CreateIndex(
                name: "IX_ApplianceModels_ApplianceCatalogId",
                table: "ApplianceModels",
                column: "ApplianceCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplianceModels_Brand",
                table: "ApplianceModels",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdProfiles_UserId",
                table: "HouseholdProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LifestyleProfiles_UserId",
                table: "LifestyleProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeterSetups_UserId",
                table: "MeterSetups",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalizationProgresses_UserId",
                table: "PersonalizationProgresses",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingTargets_UserId",
                table: "SavingTargets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAppliances_ApplianceCatalogId",
                table: "UserAppliances",
                column: "ApplianceCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAppliances_ApplianceModelId",
                table: "UserAppliances",
                column: "ApplianceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAppliances_UserId",
                table: "UserAppliances",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HouseholdProfiles");

            migrationBuilder.DropTable(
                name: "LifestyleProfiles");

            migrationBuilder.DropTable(
                name: "MeterSetups");

            migrationBuilder.DropTable(
                name: "PersonalizationProgresses");

            migrationBuilder.DropTable(
                name: "SavingTargets");

            migrationBuilder.DropTable(
                name: "UserAppliances");

            migrationBuilder.DropTable(
                name: "ApplianceModels");

            migrationBuilder.DropTable(
                name: "ApplianceCatalogs");

            migrationBuilder.DropTable(
                name: "ApplianceCategories");
        }
    }
}
