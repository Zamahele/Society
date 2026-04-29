using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocietyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProofOfPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ProofData",
                schema: "society",
                table: "MonthlyPayments",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProofFileName",
                schema: "society",
                table: "MonthlyPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ProofData",
                schema: "society",
                table: "JoiningFeePayments",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProofFileName",
                schema: "society",
                table: "JoiningFeePayments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProofData",
                schema: "society",
                table: "MonthlyPayments");

            migrationBuilder.DropColumn(
                name: "ProofFileName",
                schema: "society",
                table: "MonthlyPayments");

            migrationBuilder.DropColumn(
                name: "ProofData",
                schema: "society",
                table: "JoiningFeePayments");

            migrationBuilder.DropColumn(
                name: "ProofFileName",
                schema: "society",
                table: "JoiningFeePayments");
        }
    }
}
