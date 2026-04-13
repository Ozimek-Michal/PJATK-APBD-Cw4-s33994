using LegacyRenewalApp.Interfaces;

namespace LegacyRenewalApp;

public class DiscountByPoints : IDiscountRule
{
    public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
    {
        if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
        {
            int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
            return new DiscountResult(pointsToUse, $"loyalty points used: {pointsToUse}; ");
        }
            
        return new DiscountResult(0m, string.Empty);
    }
}