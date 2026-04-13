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
        private readonly ICustomerRepository _customerRepository;
        private readonly IRenewalPriceCalculator _priceCalculator;
        private readonly IInvoicePublisher _invoicePublisher;
        private readonly IInvoiceCreator _invoiceCreator;

        public SubscriptionRenewalService() :
            this(new CustomerRepository(),
                new PriceCalculator(
                    new SubscriptionPlanRepository(), new List<IDiscountRule> 
                    { new DiscountBySegment(), new DiscountByYears(), new DiscountBySeats(), new DiscountByPoints() }, 
                    new TaxRateProvider(), new PaymentFeeCalculator(), new SupportFeeCalculator()
                    ),
                new InvoicePublisher(new LegacyBillingGatewayAdapter(), new MailService()),
                new InvoiceCreator()){}

        public SubscriptionRenewalService(ICustomerRepository customerRepository, IRenewalPriceCalculator priceCalculator,  
            IInvoicePublisher invoicePublisher, IInvoiceCreator invoiceCreator)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _priceCalculator = priceCalculator ??  throw new ArgumentNullException(nameof(priceCalculator));
            _invoicePublisher = invoicePublisher ?? throw new ArgumentNullException(nameof(invoicePublisher));
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

        private RenewalInvoice CreateRenewalInvoice(
            RenewalRequest request)
        {
            var customer = _customerRepository.GetById(request.CustomerId);
            customer.EnsureCanRenew();
            var pricing = _priceCalculator.GetRenewalPrice(request, customer);
            var invoice = _invoiceCreator.Create(request, customer, pricing);
            _invoicePublisher.Publish(customer, invoice);

            return invoice;
        }
    }
}
