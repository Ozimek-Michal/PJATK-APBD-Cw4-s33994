namespace LegacyRenewalApp.Interfaces;

public record DiscountResult(decimal Amount, string Note);
public interface IDiscountRule
{
    DiscountResult Calculate(
        Customer customer, 
        SubscriptionPlan plan, 
        int seatCount, 
        decimal baseAmount, 
        bool useLoyaltyPoints);
}
