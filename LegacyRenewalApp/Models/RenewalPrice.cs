namespace LegacyRenewalApp.Models;

public record RenewalPrice(
    decimal BaseAmount, decimal DiscountAmount, decimal SupportFee, 
    decimal PaymentFee, decimal TaxAmount, decimal FinalAmount, string Notes);