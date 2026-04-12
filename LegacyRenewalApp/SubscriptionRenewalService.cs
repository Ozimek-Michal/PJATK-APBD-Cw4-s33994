using System;
using System.Collections.Generic;
using LegacyRenewalApp.Fees;
using LegacyRenewalApp.Interfaces;
using LegacyRenewalApp.Models;

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
        private readonly IMailService _mailService;
        private readonly IInvoiceCreator _invoiceCreator;

        public SubscriptionRenewalService() : 
            this(new LegacyBillingGatewayAdapter(),  
                new CustomerRepository(), 
                new SubscriptionPlanRepository(),
                new List<IDiscountRule>
                {
                    new DiscountBySegment(), 
                    new DiscountByYears(), 
                    new DiscountBySeats(), 
                    new DiscountByPoints()
                }, 
                new TaxRateProvider(), 
                new PaymentFeeCalculator(), 
                new SupportFeeCalculator(),
                new MailService(new LegacyBillingGatewayAdapter()),
                new InvoiceCreator()){}

        public SubscriptionRenewalService(IBillingGateway billingGateway, ICustomerRepository customerRepository, ISubsPlanRepository subsPlanRepository,  
            IEnumerable<IDiscountRule> discountRules, ITaxRateProvider taxRateProvider,  IPaymentFeeCalculator paymentFeeCalculator, 
            ISupportFeeCalculator supportFeeCalculator, IMailService mailService, IInvoiceCreator invoiceCreator)
        {
            _billingGateway = billingGateway ?? throw new ArgumentNullException(nameof(billingGateway));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _subsPlanRepository = subsPlanRepository ?? throw new ArgumentNullException(nameof(subsPlanRepository));
            _discountRules =  discountRules ?? throw new ArgumentNullException(nameof(discountRules));
            _taxRateProvider = taxRateProvider ?? throw new ArgumentNullException(nameof(taxRateProvider));
            _paymentFeeCalculator = paymentFeeCalculator ?? throw new ArgumentNullException(nameof(paymentFeeCalculator));
            _supportFeeCalculator = supportFeeCalculator ?? throw new ArgumentNullException(nameof(supportFeeCalculator));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _invoiceCreator = invoiceCreator ?? throw new ArgumentNullException(nameof(invoiceCreator));
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId, string planCode, int seatCount,
            string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints)
        {
            var request = new RenewalRequest(customerId, planCode, seatCount, paymentMethod, includePremiumSupport,
                useLoyaltyPoints);
            return CreateRenewalInvoice(request);
        }

        public RenewalInvoice CreateRenewalInvoice(
            RenewalRequest request)
        {
            var customer = _customerRepository.GetById(request.CustomerId);
            var plan = _subsPlanRepository.GetByCode(request.NormalizedPlanCode);

            customer.EnsureCanRenew();

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

            var invoice = _invoiceCreator.Create(request, customer, baseAmount, discountAmount, 
                supportFee, paymentFee, taxAmount, finalAmount, notes);
            _billingGateway.SaveInvoice(invoice);
            _mailService.SendRenewalInvoice(customer, invoice);

            return invoice;
        }
    }
}
