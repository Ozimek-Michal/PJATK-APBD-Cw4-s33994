using LegacyRenewalApp.Interfaces;
namespace LegacyRenewalApp.Models;

public class MailService : IMailService
{
    private readonly IBillingGateway _billingGateway;

    public MailService(IBillingGateway billingGateway)
    {
        _billingGateway = billingGateway;
    }

    public void SendRenewalInvoice(Customer customer, RenewalInvoice invoice)
    {
        if (string.IsNullOrWhiteSpace(customer.Email)) 
            return;

        string subject = "Subscription renewal invoice";
        
        string body = $"Hello {customer.FullName}, your renewal for plan {invoice.PlanCode} " +
                      $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

        _billingGateway.SendEmail(customer.Email, subject, body);
    }
}