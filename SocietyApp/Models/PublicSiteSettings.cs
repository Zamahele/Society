namespace SocietyApp.Models;

public class PublicSiteSettings
{
    public int Id { get; set; }
    public string OrganizationName { get; set; } = "Organization name";
    public string RegistrationNumber { get; set; } = "Not provided";
    public string EnterpriseType { get; set; } = "Not provided";
    public string EnterpriseStatus { get; set; } = "Not provided";
    public string RegistrationDate { get; set; } = "Not provided";
    public string BusinessStartDate { get; set; } = "Not provided";
    public string FinancialYearEnd { get; set; } = "Not provided";
    public string MainBusinessObject { get; set; } = "Not provided";
    public string PostalAddress { get; set; } = "Not provided";
    public string RegisteredOfficeAddress { get; set; } = "Not provided";

    public string BankName { get; set; } = "Not provided";
    public string BankAccountName { get; set; } = "Not provided";
    public string BankAccountNumber { get; set; } = "Not provided";
    public string BankBranchCode { get; set; } = "Not provided";
    public string BankAccountType { get; set; } = "Not provided";

    public string ContactAddress { get; set; } = "Not provided";
    public string ContactPhone1 { get; set; } = "Not provided";
    public string ContactPhone2 { get; set; } = "Not provided";
    public string ContactPhone3 { get; set; } = "Not provided";
    public string ContactEmailInfo { get; set; } = "Not provided";
    public string ContactEmailClaims { get; set; } = "Not provided";
}
