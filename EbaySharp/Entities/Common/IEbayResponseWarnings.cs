namespace EbaySharp.Entities.Common
{
    /// <summary>
    /// Implemented by eBay REST response models that can carry API warnings and/or errors.
    /// When eBay returns a non-success status code, the client deserializes the response body into
    /// the model and returns it (with <see cref="Errors"/> and/or <see cref="Warnings"/> populated)
    /// instead of throwing, so callers can inspect the failure details.
    /// </summary>
    public interface IEbayResponseWarnings
    {
        /// <summary>
        /// Warnings returned by the API (eBay's "warnings" collection).
        /// </summary>
        Warning[] Warnings { get; set; }

        /// <summary>
        /// Errors returned by the API when the request fails (eBay's "errors" collection).
        /// </summary>
        Error[] Errors { get; set; }
    }
}
