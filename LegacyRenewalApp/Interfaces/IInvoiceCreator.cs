using LegacyRenewalApp.Models;
namespace LegacyRenewalApp.Interfaces;

public interface IInvoiceCreator
{
    public RenewalInvoice Create(RenewalRequest request,
        Customer customer,
        decimal baseAmount,
        decimal discountAmount,
        decimal supportFee,
        decimal paymentFee,
        decimal taxAmount,
        decimal finalAmount,
        string notes);
}