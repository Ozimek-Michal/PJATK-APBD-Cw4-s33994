namespace LegacyRenewalApp.Interfaces;

public interface IMailService
{ 
    public (string Subject, string Body)? PrepareInvoiceEmail(Customer customer, RenewalInvoice invoice);
    
}