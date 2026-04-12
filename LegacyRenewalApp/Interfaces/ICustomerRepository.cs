namespace LegacyRenewalApp.Interfaces;

public interface ICustomerRepository
{
    public Customer GetById(int customerId);
}