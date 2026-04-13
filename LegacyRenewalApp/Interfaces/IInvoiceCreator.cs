using LegacyRenewalApp.Models;
namespace LegacyRenewalApp.Interfaces;

public interface IInvoiceCreator
{
    public RenewalInvoice Create(RenewalRequest request,
        Customer customer, RenewalPrice renewalPrice);
}