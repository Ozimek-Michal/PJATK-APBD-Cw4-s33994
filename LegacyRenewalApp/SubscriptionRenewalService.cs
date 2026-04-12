using System;
using System.Collections.Generic;
using LegacyRenewalApp.Fees;
using LegacyRenewalApp.Interfaces;

//TODO
// ładnie podzielić kod na odpowiedzialności,
// poprawić kohezję klas,
// zmniejszyć coupling,
// usunąć powtórzenia,
// wydzielić fragmenty logiki do osobnych klas,
// rozważyć rozbicie części if-else na interfejsy i implementacje zgodnie z Open/Closed Principle,


namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly IBillingGateway _billingGateway;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubsPlanRepository  _subsPlanRepository;
        private readonly IEnumerable<IDiscountRule> _discountRules;
        private readonly ITaxRateProvider _taxRateProvider;
        private readonly IPaymentFeeCalculator _paymentFeeCalculator;
        private readonly ISupportFeeCalculator _supportFeeCalculator;

        public SubscriptionRenewalService() : this(new LegacyBillingGatewayAdapter(),  new CustomerRepository(), new SubscriptionPlanRepository(), new List<IDiscountRule>{
            new DiscountBySegment(), new DiscountByYears(), new DiscountBySeats(), new DiscountByPoints()}, new TaxRateProvider(), new PaymentFeeCalculator(), new SupportFeeCalculator()){}

        public SubscriptionRenewalService(IBillingGateway billingGateway, ICustomerRepository customerRepository, ISubsPlanRepository subsPlanRepository,  
            IEnumerable<IDiscountRule> discountRules, ITaxRateProvider taxRateProvider,  IPaymentFeeCalculator paymentFeeCalculator, ISupportFeeCalculator supportFeeCalculator)
        {
            _billingGateway = billingGateway ?? throw new ArgumentNullException(nameof(billingGateway));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _subsPlanRepository = subsPlanRepository ?? throw new ArgumentNullException(nameof(subsPlanRepository));
            _discountRules =  discountRules ?? throw new ArgumentNullException(nameof(discountRules));
            _taxRateProvider = taxRateProvider ?? throw new ArgumentNullException(nameof(taxRateProvider));
            _paymentFeeCalculator = paymentFeeCalculator ?? throw new ArgumentNullException(nameof(paymentFeeCalculator));
            _supportFeeCalculator = supportFeeCalculator ?? throw new ArgumentNullException(nameof(supportFeeCalculator));
        }
        
        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer id must be positive");
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new ArgumentException("Plan code is required");
            }

            if (seatCount <= 0)
            {
                throw new ArgumentException("Seat count must be positive");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();
            
            var customer = _customerRepository.GetById(customerId);
            var plan = _subsPlanRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            string notes = string.Empty;

            foreach (var rule in _discountRules)
            {
                var result = rule.Calculate(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
                discountAmount += result.Amount;
                notes += result.Note; 
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            var supportFeeResult = _supportFeeCalculator.Calculate(includePremiumSupport, normalizedPlanCode);
            decimal supportFee = supportFeeResult.Amount;
            notes += supportFeeResult.Note;

            var paymentFeeResult = _paymentFeeCalculator.Calculate(normalizedPaymentMethod, subtotalAfterDiscount + supportFee);
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

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            _billingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                _billingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
