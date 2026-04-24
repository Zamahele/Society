namespace SocietyApp.Models;

public class PublicSiteSettings
{
    public int Id { get; set; }
    public string OrganizationName { get; set; } = "Give Compassion NPC";
    public string RegistrationNumber { get; set; } = "K2026009248 / 2026/009248/08";
    public string EnterpriseType { get; set; } = "Non Profit Company";
    public string EnterpriseStatus { get; set; } = "In Business";
    public string RegistrationDate { get; set; } = "09/01/2026";
    public string BusinessStartDate { get; set; } = "09/01/2026";
    public string FinancialYearEnd { get; set; } = "June";
    public string MainBusinessObject { get; set; } = "Business activities not restricted.";
    public string PostalAddress { get; set; } = "D1707 Mashona, Mahlabathini, Ulundi, Kwa-Zulu Natal, 3838";
    public string RegisteredOfficeAddress { get; set; } = "D1707 Mashona, Mahlabathini, Ulundi, Kwa-Zulu Natal, 3838";

    public string BankName { get; set; } = "Capitec";
    public string BankAccountName { get; set; } = "Give Compassion NPC";
    public string BankAccountNumber { get; set; } = "1054981680";
    public string BankBranchCode { get; set; } = "450105";
    public string BankAccountType { get; set; } = "Current";

    public string ContactAddress { get; set; } = "D1707 Mashona, Mahlabathini, Ulundi, Kwa-Zulu Natal, 3838";
    public string ContactPhone1 { get; set; } = "073 105 4674";
    public string ContactPhone2 { get; set; } = "060 708 1052";
    public string ContactPhone3 { get; set; } = "076 738 4252";
    public string ContactEmailInfo { get; set; } = "No official email provided";
    public string ContactEmailClaims { get; set; } = "Please contact committee by phone";
}
