using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocietyApp.Migrations
{
    /// <inheritdoc />
    public partial class MigratePendingPaymentToPending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PendingPayment (4) was retired from the approval flow.
            // Legacy rows are folded back into Pending (0) so they appear
            // and behave as plain Pending members in the new flow.
            migrationBuilder.Sql("UPDATE [society].[Memberships] SET [Status] = 0 WHERE [Status] = 4;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no-op. We cannot reliably tell which of the now-Pending
            // rows used to be PendingPayment, so reversing the data fold-back is
            // not safe. Schema is unchanged either way.
        }
    }
}
