using LegacyRenewalApp.Interfaces;

namespace LegacyRenewalApp;

public class DiscountByYears : IDiscountRule
{
    public DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
    {
        if (customer.YearsWithCompany >= 5) return new DiscountResult(baseAmount * 0.07m, "long-term loyalty discount; ");
        if (customer.YearsWithCompany >= 2) return new DiscountResult(baseAmount * 0.03m, "basic loyalty discount; ");
            
        return new DiscountResult(0m, string.Empty);
    }
}