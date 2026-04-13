using LegacyRenewalApp.Interfaces;
using System;
namespace LegacyRenewalApp.Models;

public class InvoiceCreator :  IInvoiceCreator
{
    public RenewalInvoice Create(
        RenewalRequest request, 
        Customer customer, 
        RenewalPrice renewalPrice)
    {
        return new RenewalInvoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{request.CustomerId}-{request.NormalizedPlanCode}",
            CustomerName = customer.FullName,
            PlanCode = request.NormalizedPlanCode,
            PaymentMethod = request.NormalizedPaymentMethod,
            SeatCount = request.SeatCount,
            
            BaseAmount = Round(renewalPrice.BaseAmount),
            DiscountAmount = Round(renewalPrice.DiscountAmount),
            SupportFee = Round(renewalPrice.SupportFee),
            PaymentFee = Round(renewalPrice.PaymentFee),
            TaxAmount = Round(renewalPrice.TaxAmount),
            FinalAmount = Round(renewalPrice.FinalAmount),
                
            Notes = renewalPrice.Notes.Trim(),
            GeneratedAt = DateTime.UtcNow
        };
    }
    private decimal Round(decimal amount) 
        => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    
}