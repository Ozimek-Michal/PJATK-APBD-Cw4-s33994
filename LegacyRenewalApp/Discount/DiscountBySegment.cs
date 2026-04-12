using LegacyRenewalApp.Interfaces;

namespace LegacyRenewalApp;

public class DiscountBySegment : IDiscountRule
{
    public DiscountResult Calculate(
        Customer customer, 
        SubscriptionPlan plan, 
        int seatCount, 
        decimal baseAmount, 
        bool useLoyaltyPoints)
    {
        if (customer.Segment == "Silver") 
            return new DiscountResult(baseAmount * 0.05m, "silver discount; ");
                
        if (customer.Segment == "Gold") 
            return new DiscountResult(baseAmount * 0.10m, "gold discount; ");
                
        if (customer.Segment == "Platinum") 
            return new DiscountResult(baseAmount * 0.15m, "platinum discount; ");
                
        if (customer.Segment == "Education" && plan.IsEducationEligible) 
            return new DiscountResult(baseAmount * 0.20m, "education discount; ");
        
        return new DiscountResult(0m, string.Empty);
    }
    
}