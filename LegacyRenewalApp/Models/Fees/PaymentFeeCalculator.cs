using LegacyRenewalApp.Interfaces;
using System;

namespace LegacyRenewalApp.Fees;

public class PaymentFeeCalculator : IPaymentFeeCalculator
{
    public FeeResult Calculate(string paymentMethod, decimal baseAmountForFee)
    {
        return paymentMethod switch
        {
            "CARD" => new FeeResult(baseAmountForFee * 0.02m, "card payment fee; "),
            "BANK_TRANSFER" => new FeeResult(baseAmountForFee * 0.01m, "bank transfer fee; "),
            "PAYPAL" => new FeeResult(baseAmountForFee * 0.035m, "paypal fee; "),
            "INVOICE" => new FeeResult(0m, "invoice payment; "),
            _ => throw new ArgumentException("Unsupported payment method")
        };
    }
}