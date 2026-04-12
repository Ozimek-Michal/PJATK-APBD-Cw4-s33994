namespace LegacyRenewalApp.Interfaces;

public interface IMailService
{ 
    void SendRenewalInvoice(Customer customer, RenewalInvoice invoice);
    
}