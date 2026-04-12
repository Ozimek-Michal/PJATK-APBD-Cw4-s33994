namespace LegacyRenewalApp.Interfaces;

public interface IDiscountRule
{
    DiscountResult Calculate(
        Customer customer, 
        SubscriptionPlan plan, 
        int seatCount, 
        decimal baseAmount, 
        bool useLoyaltyPoints);
}

public class DiscountResult
{
    public decimal Amount { get; }
    public string Note { get; }

    public DiscountResult(decimal amount, string note)
    {
        Amount = amount;
        Note = note;
    }
}