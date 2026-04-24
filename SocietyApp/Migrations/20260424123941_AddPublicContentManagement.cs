using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocietyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicContentManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommitteeMembers",
                schema: "society",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RoleTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicSiteSettings",
                schema: "society",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnterpriseType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EnterpriseStatus = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    RegistrationDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessStartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinancialYearEnd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainBusinessObject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisteredOfficeAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankBranchCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmailInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmailClaims = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSiteSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitteeMembers",
                schema: "society");

            migrationBuilder.DropTable(
                name: "PublicSiteSettings",
                schema: "society");
        }
    }
}
