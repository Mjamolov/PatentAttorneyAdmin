using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatentAttorneyAdmin.Data.Migrations
{
    public partial class AddDocumentCorrectionFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsCorrection",
                table: "ServiceApplicationDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StaffComment",
                table: "ServiceApplicationDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsCorrection",
                table: "ServiceApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "StaffComment",
                table: "ServiceApplicationDocuments");
        }
    }
}
