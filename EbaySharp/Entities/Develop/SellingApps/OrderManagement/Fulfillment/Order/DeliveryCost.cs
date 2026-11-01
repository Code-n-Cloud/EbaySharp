using EbaySharp.Entities.Common;

namespace EbaySharp.Entities.Develop.SellingApps.OrderManagement.Fulfillment.Order
{
    public class DeliveryCost : Amount
    {
        public Amount DiscountAmount { get; set; }
        public Amount HandlingCost { get; set; }
        public Amount ImportCharges { get; set; }
        public Amount ShippingCost { get; set; }
        public Amount ShippingIntermediationFee { get; set; }
    }
}