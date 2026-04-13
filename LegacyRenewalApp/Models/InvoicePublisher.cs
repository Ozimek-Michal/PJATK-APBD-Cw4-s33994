using LegacyRenewalApp.Interfaces;
namespace LegacyRenewalApp.Models;

public class InvoicePublisher : IInvoicePublisher
{
    private readonly IBillingGateway _billingGateway;
    private readonly IMailService _mailService;

    public InvoicePublisher(IBillingGateway billingGateway, IMailService mailService)
    {
        _billingGateway = billingGateway;
        _mailService = mailService;
    }

    public void Publish(Customer customer, RenewalInvoice invoice)
    {
        _billingGateway.SaveInvoice(invoice);
        if (_mailService.PrepareInvoiceEmail(customer, invoice) is { } email)
            _billingGateway.SendEmail(customer.Email, email.Subject, email.Body);
    }
}