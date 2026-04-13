using LegacyRenewalApp.Interfaces;
namespace LegacyRenewalApp.Models;

public class MailService : IMailService
{
    public (string Subject, string Body)? PrepareInvoiceEmail(Customer customer, RenewalInvoice invoice)
    {
        if (string.IsNullOrWhiteSpace(customer.Email))
            return null;

        string subject = "Subscription renewal invoice";
        string body = $"Hello {customer.FullName}, your renewal for plan {invoice.PlanCode} " +
                      $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

        return (subject, body);
    }
}