using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocietyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSocietySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "society");

            migrationBuilder.RenameTable(
                name: "MonthlyPayments",
                newName: "MonthlyPayments",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "Memberships",
                newName: "Memberships",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "MemberDependants",
                newName: "MemberDependants",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "JoiningFeePayments",
                newName: "JoiningFeePayments",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "DeathClaims",
                newName: "DeathClaims",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "society");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "society");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MonthlyPayments",
                schema: "society",
                newName: "MonthlyPayments");

            migrationBuilder.RenameTable(
                name: "Memberships",
                schema: "society",
                newName: "Memberships");

            migrationBuilder.RenameTable(
                name: "MemberDependants",
                schema: "society",
                newName: "MemberDependants");

            migrationBuilder.RenameTable(
                name: "JoiningFeePayments",
                schema: "society",
                newName: "JoiningFeePayments");

            migrationBuilder.RenameTable(
                name: "DeathClaims",
                schema: "society",
                newName: "DeathClaims");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "society",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "society",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "society",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "society",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "society",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "society",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "society",
                newName: "AspNetRoleClaims");
        }
    }
}
