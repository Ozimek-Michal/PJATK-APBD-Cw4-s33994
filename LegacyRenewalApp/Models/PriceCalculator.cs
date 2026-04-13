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
        decimal baseAmount = (plan.MonthlyPricePerSeat * request.SeatCount * 12m) + plan.SetupFee;
        decimal discountAmount = 0m;
        string notes = string.Empty;

        foreach (var rule in _discountRules)
        {
            var result = rule.Calculate(customer, plan, request.SeatCount, baseAmount, request.UseLoyaltyPoints);
            discountAmount += result.Amount;
            notes += result.Note; 
        }

        decimal subtotalAfterDiscount = baseAmount - discountAmount;
        if (subtotalAfterDiscount < 300m)
        {
            subtotalAfterDiscount = 300m;
            notes += "minimum discounted subtotal applied; ";
        }

        var supportFeeResult = _supportFeeCalculator.Calculate(request.IncludePremiumSupport, request.NormalizedPlanCode);
        decimal supportFee = supportFeeResult.Amount;
        notes += supportFeeResult.Note;

        var paymentFeeResult = _paymentFeeCalculator.Calculate(request.NormalizedPaymentMethod, subtotalAfterDiscount + supportFee);
        decimal paymentFee = paymentFeeResult.Amount;
        notes += paymentFeeResult.Note;

        decimal taxRate = _taxRateProvider.GetTaxRate(customer.Country);

        decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
        decimal taxAmount = taxBase * taxRate;
        decimal finalAmount = taxBase + taxAmount;
            

        if (finalAmount < 500m)
        {
            finalAmount = 500m;
            notes += "minimum invoice amount applied; ";
        }
        
        return new RenewalPrice(baseAmount, discountAmount, supportFee, paymentFee, taxAmount, finalAmount, notes.Trim());
    }
}