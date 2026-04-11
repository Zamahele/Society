using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocietyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddClerkSubmittedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmittedByClerkId",
                schema: "society",
                table: "MonthlyPayments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedByClerkId",
                schema: "society",
                table: "JoiningFeePayments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedByClerkId",
                schema: "society",
                table: "DeathClaims",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPayments_SubmittedByClerkId",
                schema: "society",
                table: "MonthlyPayments",
                column: "SubmittedByClerkId");

            migrationBuilder.CreateIndex(
                name: "IX_JoiningFeePayments_SubmittedByClerkId",
                schema: "society",
                table: "JoiningFeePayments",
                column: "SubmittedByClerkId");

            migrationBuilder.CreateIndex(
                name: "IX_DeathClaims_SubmittedByClerkId",
                schema: "society",
                table: "DeathClaims",
                column: "SubmittedByClerkId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeathClaims_AspNetUsers_SubmittedByClerkId",
                schema: "society",
                table: "DeathClaims",
                column: "SubmittedByClerkId",
                principalSchema: "society",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JoiningFeePayments_AspNetUsers_SubmittedByClerkId",
                schema: "society",
                table: "JoiningFeePayments",
                column: "SubmittedByClerkId",
                principalSchema: "society",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyPayments_AspNetUsers_SubmittedByClerkId",
                schema: "society",
                table: "MonthlyPayments",
                column: "SubmittedByClerkId",
                principalSchema: "society",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeathClaims_AspNetUsers_SubmittedByClerkId",
                schema: "society",
                table: "DeathClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_JoiningFeePayments_AspNetUsers_SubmittedByClerkId",
                schema: "society",
                table: "JoiningFeePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyPayments_AspNetUsers_SubmittedByClerkId",
                schema: "society",
                table: "MonthlyPayments");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyPayments_SubmittedByClerkId",
                schema: "society",
                table: "MonthlyPayments");

            migrationBuilder.DropIndex(
                name: "IX_JoiningFeePayments_SubmittedByClerkId",
                schema: "society",
                table: "JoiningFeePayments");

            migrationBuilder.DropIndex(
                name: "IX_DeathClaims_SubmittedByClerkId",
                schema: "society",
                table: "DeathClaims");

            migrationBuilder.DropColumn(
                name: "SubmittedByClerkId",
                schema: "society",
                table: "MonthlyPayments");

            migrationBuilder.DropColumn(
                name: "SubmittedByClerkId",
                schema: "society",
                table: "JoiningFeePayments");

            migrationBuilder.DropColumn(
                name: "SubmittedByClerkId",
                schema: "society",
                table: "DeathClaims");
        }
    }
}
