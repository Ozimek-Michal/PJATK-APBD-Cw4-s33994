namespace LegacyRenewalApp.Interfaces;

public interface IInvoicePublisher
{
    void Publish(Customer customer, RenewalInvoice invoice);
}