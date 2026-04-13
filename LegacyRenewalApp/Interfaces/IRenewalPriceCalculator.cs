using LegacyRenewalApp.Models;

namespace LegacyRenewalApp.Interfaces;

public interface IRenewalPriceCalculator
{
    public RenewalPrice GetRenewalPrice(RenewalRequest renewalRequest, Customer customer);
    
}