using LegacyRenewalApp.Interfaces;
using System;

namespace LegacyRenewalApp.Fees;

public class SupportFeeCalculator : ISupportFeeCalculator
{
    public FeeResult Calculate(bool includePremiumSupport, string planCode)
    {
        if (!includePremiumSupport) 
            return new FeeResult(0m, string.Empty);

        decimal fee = planCode switch
        {
            "START" => 250m,
            "PRO" => 400m,
            "ENTERPRISE" => 700m,
            _ => 0m
        };

        return new FeeResult(fee, fee > 0 ? "premium support included; " : string.Empty);
    }
}
