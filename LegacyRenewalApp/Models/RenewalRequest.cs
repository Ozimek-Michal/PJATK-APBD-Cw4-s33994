namespace LegacyRenewalApp.Models;
using System;

public record RenewalRequest(
    int CustomerId,
    string PlanCode,
    int SeatCount,
    string PaymentMethod,
    bool IncludePremiumSupport,
    bool UseLoyaltyPoints)
{
    
    public void Validate()
    {
        if (CustomerId <= 0) throw new ArgumentException("Customer id must be positive");
        if (string.IsNullOrWhiteSpace(PlanCode)) throw new ArgumentException("Plan code is required");
        if (SeatCount <= 0) throw new ArgumentException("Seat count must be positive");
        if (string.IsNullOrWhiteSpace(PaymentMethod)) throw new ArgumentException("Payment method is required");
        
    }

    public string NormalizedPlanCode => PlanCode.Trim().ToUpperInvariant();
    public string NormalizedPaymentMethod => PaymentMethod.Trim().ToUpperInvariant();
}