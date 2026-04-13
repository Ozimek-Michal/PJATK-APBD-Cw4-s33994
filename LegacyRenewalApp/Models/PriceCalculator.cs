using System.Collections.Generic;
using LegacyRenewalApp.Interfaces;

namespace LegacyRenewalApp.Models;

public class PriceCalculator : IRenewalPriceCalculator
{
    private readonly ISubsPlanRepository _subsPlanRepository;
    private readonly IEnumerable<IDiscountRule> _discountRules;
    private readonly ITaxRateProvider _taxRateProvider;
    private readonly IPaymentFeeCalculator _paymentFeeCalculator;
    private readonly ISupportFeeCalculator _supportFeeCalculator;

    public PriceCalculator(
        ISubsPlanRepository subsPlanRepository,
        IEnumerable<IDiscountRule> discountRules,
        ITaxRateProvider taxRateProvider,
        IPaymentFeeCalculator paymentFeeCalculator,
        ISupportFeeCalculator supportFeeCalculator)
    {
        _subsPlanRepository = subsPlanRepository;
        _discountRules = discountRules;
        _taxRateProvider = taxRateProvider;
        _paymentFeeCalculator = paymentFeeCalculator;
        _supportFeeCalculator = supportFeeCalculator;
    }
    
    public RenewalPrice GetRenewalPrice(RenewalRequest request, Customer customer)
    {
        var plan = _subsPlanRepository.GetByCode(request.PlanCode);   
        var notes = new List<string>();
        decimal Record((decimal Amount, string Note) result)
        {
            if (!string.IsNullOrWhiteSpace(result.Note)) 
                notes.Add(result.Note);
            return result.Amount;
        }
        
        decimal baseAmount = CalculateBaseAmount(plan, request.SeatCount);
        decimal discountAmount = Record(CalculateDiscounts(customer, plan, request, baseAmount));
        
        decimal subtotal = Record(MinimumLimit(baseAmount - discountAmount, 300m, "minimum discounted subtotal applied;"));
        decimal supportFee = Record(CalculateSupportFee(request));
        decimal paymentFee = Record(CalculatePaymentFee(request, subtotal + supportFee));
       
        decimal taxBase = subtotal + supportFee + paymentFee;
        decimal taxAmount = taxBase * _taxRateProvider.GetTaxRate(customer.Country);

        decimal finalAmount = Record(MinimumLimit(taxBase + taxAmount, 500m, "minimum invoice amount applied;"));

        return new RenewalPrice(
            baseAmount, 
            discountAmount, 
            supportFee, 
            paymentFee, 
            taxAmount, 
            finalAmount, 
            string.Join(" ", notes).Trim());
    }
    
    private decimal CalculateBaseAmount(SubscriptionPlan plan, int seatCount)
    {
        return (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
    }

    private (decimal Amount, string Notes) CalculateDiscounts(Customer customer, SubscriptionPlan plan, RenewalRequest request, decimal baseAmount)
    {
        decimal totalDiscount = 0m;
        string combinedNotes = string.Empty;

        foreach (var rule in _discountRules)
        {
            var result = rule.Calculate(customer, plan, request.SeatCount, baseAmount, request.UseLoyaltyPoints);
            totalDiscount += result.Amount;
            
            if (!string.IsNullOrWhiteSpace(result.Note)) 
                combinedNotes += result.Note.Trim() + " ";
        }
        
        return (totalDiscount, combinedNotes.Trim());
    }

    private (decimal Amount, string Note) CalculateSupportFee(RenewalRequest request)
    {
        var result = _supportFeeCalculator.Calculate(request.IncludePremiumSupport, request.NormalizedPlanCode);
        return (result.Amount, result.Note?.Trim() ?? string.Empty);
    }

    private (decimal Amount, string Note) CalculatePaymentFee(RenewalRequest request, decimal currentTotal)
    {
        var result = _paymentFeeCalculator.Calculate(request.NormalizedPaymentMethod, currentTotal);
        return (result.Amount, result.Note?.Trim() ?? string.Empty);
    }

    private (decimal Amount, string Note) MinimumLimit(decimal currentAmount, decimal minimumAllowed, string noteIfApplied)
    {
        if (currentAmount >= minimumAllowed) 
            return (currentAmount, string.Empty);
            
        return (minimumAllowed, noteIfApplied);
    }
}