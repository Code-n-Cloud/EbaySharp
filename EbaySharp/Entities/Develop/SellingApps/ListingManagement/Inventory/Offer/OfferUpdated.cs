using EbaySharp.Entities.Common;

namespace EbaySharp.Entities.Develop.SellingApps.ListingManagement.Inventory.Offer
{
    public class OfferUpdated : IEbayResponseWarnings
    {
        public string OfferId { get; set; }
        public Warning[] Warnings { get; set; }
        public Error[] Errors { get; set; }
    }
}
