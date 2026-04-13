using LegacyRenewalApp.Interfaces;

namespace LegacyRenewalApp;

public class DiscountBySeats : IDiscountRule
{
    public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
    {
        if (seatCount >= 50) return new DiscountResult(baseAmount * 0.12m, "large team discount; ");
        if (seatCount >= 20) return new DiscountResult(baseAmount * 0.08m, "medium team discount; ");
        if (seatCount >= 10) return new DiscountResult(baseAmount * 0.04m, "small team discount; ");
            
        return new DiscountResult(0m, string.Empty);
    }
}