using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatentAttorneyAdmin.Data.Migrations
{
    public partial class AddUserPassportExtendedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('AspNetUsers', 'ActualAddress') IS NULL
    ALTER TABLE AspNetUsers ADD ActualAddress nvarchar(max) NOT NULL CONSTRAINT DF_AspNetUsers_ActualAddress DEFAULT '';

IF COL_LENGTH('AspNetUsers', 'Inn') IS NULL
    ALTER TABLE AspNetUsers ADD Inn nvarchar(max) NOT NULL CONSTRAINT DF_AspNetUsers_Inn DEFAULT '';

IF COL_LENGTH('AspNetUsers', 'PassportAddress') IS NULL
    ALTER TABLE AspNetUsers ADD PassportAddress nvarchar(max) NOT NULL CONSTRAINT DF_AspNetUsers_PassportAddress DEFAULT '';

IF COL_LENGTH('AspNetUsers', 'PassportExpiryDate') IS NULL
    ALTER TABLE AspNetUsers ADD PassportExpiryDate datetime2 NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('AspNetUsers', 'PassportExpiryDate') IS NOT NULL
    ALTER TABLE AspNetUsers DROP COLUMN PassportExpiryDate;

IF COL_LENGTH('AspNetUsers', 'PassportAddress') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_AspNetUsers_PassportAddress')
        ALTER TABLE AspNetUsers DROP CONSTRAINT DF_AspNetUsers_PassportAddress;
    ALTER TABLE AspNetUsers DROP COLUMN PassportAddress;
END

IF COL_LENGTH('AspNetUsers', 'Inn') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_AspNetUsers_Inn')
        ALTER TABLE AspNetUsers DROP CONSTRAINT DF_AspNetUsers_Inn;
    ALTER TABLE AspNetUsers DROP COLUMN Inn;
END

IF COL_LENGTH('AspNetUsers', 'ActualAddress') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_AspNetUsers_ActualAddress')
        ALTER TABLE AspNetUsers DROP CONSTRAINT DF_AspNetUsers_ActualAddress;
    ALTER TABLE AspNetUsers DROP COLUMN ActualAddress;
END
");
        }
    }
}
