namespace LegacyRenewalApp.Interfaces;

public interface ISubsPlanRepository
{
    public SubscriptionPlan GetByCode(string code);
}