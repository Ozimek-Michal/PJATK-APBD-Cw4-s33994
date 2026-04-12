using LegacyRenewalApp.Interfaces;
using System;
namespace LegacyRenewalApp.Models;

public class InvoiceCreator :  IInvoiceCreator
{
    public RenewalInvoice Create(
        RenewalRequest request, 
        Customer customer, 
        decimal baseAmount, 
        decimal discountAmount, 
        decimal supportFee, 
        decimal paymentFee, 
        decimal taxAmount, 
        decimal finalAmount, 
        string notes)
    {
        return new RenewalInvoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{request.CustomerId}-{request.NormalizedPlanCode}",
            CustomerName = customer.FullName,
            PlanCode = request.NormalizedPlanCode,
            PaymentMethod = request.NormalizedPaymentMethod,
            SeatCount = request.SeatCount,
            
            BaseAmount = Round(baseAmount),
            DiscountAmount = Round(discountAmount),
            SupportFee = Round(supportFee),
            PaymentFee = Round(paymentFee),
            TaxAmount = Round(taxAmount),
            FinalAmount = Round(finalAmount),
                
            Notes = notes.Trim(),
            GeneratedAt = DateTime.UtcNow
        };
    }
    private decimal Round(decimal amount) 
        => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    
}