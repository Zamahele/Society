using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SocietyApp.Models;

namespace SocietyApp.Documents;

public class ApplicationFormPdfDocument : IDocument
{
    private readonly PublicSiteSettings _settings;

    public ApplicationFormPdfDocument(PublicSiteSettings settings)
    {
        _settings = settings;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Content().Column(column =>
            {
                column.Spacing(8);

                ComposeHeader(column);
                ComposeApplicantSection(column);
                ComposeDependantsSection(column);
                ComposeBankingSection(column);
                ComposeDeclarationSection(column);
            });
        });
    }

    private void ComposeHeader(ColumnDescriptor column)
    {
        column.Item().Border(1).Padding(8).Column(head =>
        {
            head.Spacing(4);

            head.Item().Text(GetValue(_settings.OrganizationName, "Organization name"))
                .Bold().FontSize(18).FontColor(Colors.Red.Medium);

            head.Item().Row(row =>
            {
                row.Spacing(12);

                row.RelativeItem().Text($"Reg No: {GetValue(_settings.RegistrationNumber)}");
                row.RelativeItem().Text($"Address: {GetValue(_settings.RegisteredOfficeAddress)}");
            });

            head.Item().Row(row =>
            {
                row.Spacing(12);

                row.RelativeItem().Text($"Cell: {GetValue(_settings.ContactPhone1)}");
                row.RelativeItem().Text($"Alt: {GetValue(_settings.ContactPhone2)}");
                row.RelativeItem().Text($"Alt: {GetValue(_settings.ContactPhone3)}");
            });
        });

        column.Item().Text("Application Form").Bold().FontSize(13).FontColor(Colors.Red.Darken1);
    }

    private void ComposeApplicantSection(ColumnDescriptor column)
    {
        column.Item().Text("Name / Address").Bold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(1.2f);
                cols.RelativeColumn(1.8f);
                cols.RelativeColumn(1.2f);
                cols.RelativeColumn(1.8f);
            });

            AddHeaderCell(table, "Name");
            AddHeaderCell(table, "");
            AddHeaderCell(table, "Mrs.");
            AddHeaderCell(table, "Mr.");

            AddCell(table, "Citizenship:");
            AddCell(table, "");
            AddCell(table, "ID No:");
            AddCell(table, "");

            AddCell(table, "Address:");
            AddCell(table, "");
            AddCell(table, "");
            AddCell(table, "");

            AddCell(table, "City:");
            AddCell(table, "");
            AddCell(table, "Zip:");
            AddCell(table, "");

            AddCell(table, "");
            AddCell(table, "");
            AddCell(table, "Phone:");
            AddCell(table, "");
        });
    }

    private void ComposeDependantsSection(ColumnDescriptor column)
    {
        column.Item().PaddingTop(8).Text("Dependants").Bold().FontColor(Colors.Red.Darken1);

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2);
                cols.RelativeColumn(2);
                cols.RelativeColumn(1.5f);
            });

            AddHeaderCell(table, "Name");
            AddHeaderCell(table, "Surname");
            AddHeaderCell(table, "ID no");

            for (var i = 0; i < 8; i++)
            {
                AddCell(table, " ");
                AddCell(table, " ");
                AddCell(table, " ");
            }
        });
    }

    private void ComposeBankingSection(ColumnDescriptor column)
    {
        column.Item().PaddingTop(8).Text("Banking Details").Bold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(1.3f);
                cols.RelativeColumn(2.2f);
            });

            AddCell(table, "Bank Name");
            AddCell(table, $": {GetValue(_settings.BankName)}");

            AddCell(table, "Bank Holder");
            AddCell(table, $": {GetValue(_settings.BankAccountName)}");

            AddCell(table, "Account number");
            AddCell(table, $": {GetValue(_settings.BankAccountNumber)}");

            AddCell(table, "Branch code");
            AddCell(table, $": {GetValue(_settings.BankBranchCode)}");

            AddCell(table, "Account Type");
            AddCell(table, $": {GetValue(_settings.BankAccountType)}");

            AddCell(table, "Reference");
            AddCell(table, ": Your membership No");
        });
    }

    private void ComposeDeclarationSection(ColumnDescriptor column)
    {
        column.Item().PaddingTop(8).Text("Oath and Declaration").Bold();

        column.Item().Text("I hereby certify that the information contained herein is complete and accurate. This information has been furnished with the understanding that it is to be used to determine the amount and conditions of the credit to be extended.")
            .FontSize(8);

        column.Item().PaddingTop(12).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().LineHorizontal(1);
                col.Item().Text("Signature").FontSize(9);
            });

            row.ConstantItem(24);

            row.RelativeItem().Column(col =>
            {
                col.Item().LineHorizontal(1);
                col.Item().Text("Date").FontSize(9);
            });
        });
    }

    private static void AddHeaderCell(TableDescriptor table, string value)
    {
        table.Cell().Border(1).Padding(2).Text(value).Bold().FontSize(9);
    }

    private static void AddCell(TableDescriptor table, string value)
    {
        table.Cell().Border(1).Padding(2).Text(value).FontSize(9);
    }

    private static string GetValue(string? value, string fallback = "Not provided")
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
