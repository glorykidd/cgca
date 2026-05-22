using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cgca.web.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorshipTrackingAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAddedToSystem",
                table: "SponsorshipSubmissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "SponsorshipSubmissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsContacted",
                table: "SponsorshipSubmissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeclined",
                table: "SponsorshipSubmissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SponsorshipNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SponsorshipSubmissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipNotes_SponsorshipSubmissions_SponsorshipSubmissionId",
                        column: x => x.SponsorshipSubmissionId,
                        principalTable: "SponsorshipSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipNotes_SponsorshipSubmissionId",
                table: "SponsorshipNotes",
                column: "SponsorshipSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SponsorshipNotes");

            migrationBuilder.DropColumn(
                name: "IsAddedToSystem",
                table: "SponsorshipSubmissions");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "SponsorshipSubmissions");

            migrationBuilder.DropColumn(
                name: "IsContacted",
                table: "SponsorshipSubmissions");

            migrationBuilder.DropColumn(
                name: "IsDeclined",
                table: "SponsorshipSubmissions");
        }
    }
}
