namespace LegacyRenewalApp.Interfaces;

public record FeeResult(decimal Amount, string Note);

public interface ISupportFeeCalculator
{
    FeeResult Calculate(bool includePremiumSupport, string planCode);
}

public interface IPaymentFeeCalculator
{
    FeeResult Calculate(string paymentMethod, decimal baseAmountForFee);
}