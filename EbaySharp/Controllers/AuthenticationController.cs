using EbaySharp.Entities.Identity;
using EbaySharp.Source;
using System.Text;
using System.Web;

namespace EbaySharp.Controllers
{
    public class AuthenticationController
    {
        public async Task<string> GetRefreshToken(string clinetId, string clientSecret, string authSuccessPageURL, string RUName)
        {
            var parsed = HttpUtility.ParseQueryString(authSuccessPageURL);
            var collection = new List<KeyValuePair<string, string>>
            {
                new("redirect_uri", RUName),
                new("grant_type", "authorization_code"),
                new("code", HttpUtility.UrlDecode(parsed["code"]))
            };
            ClientCredentials clientCredentialsResponse = await RequestToken(clinetId, clientSecret, collection);
            if (string.IsNullOrEmpty(clientCredentialsResponse.RefreshToken))
            {
                throw new Exception("eBay did not return a refresh token. Check that the authorization code has not expired or been used already, and that the RUName matches the one registered for this application.");
            }
            return clientCredentialsResponse.RefreshToken;
        }
        public async Task<ClientCredentials> GetClientCredentials(string clinetId, string clientSecret, string refreshToken, string scope)
        {
            var collection = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "refresh_token"),
                new("refresh_token",refreshToken),
                new("scope", scope)
            };
            ClientCredentials clientCredentials = await RequestToken(clinetId, clientSecret, collection);
            if (string.IsNullOrEmpty(clientCredentials.AccessToken))
            {
                throw new Exception("eBay did not return an access token. The client id/secret or the refresh token may be invalid, expired or issued to another account, or the requested scopes may not have been granted.");
            }
            return clientCredentials;
        }
        private static async Task<ClientCredentials> RequestToken(string clinetId, string clientSecret, List<KeyValuePair<string, string>> keyValuePayload)
        {
            string requestUrl = $"{Constants.API_SERVER_URL}{Constants.AUTHENTICATION.ENDPOINT_URL}{Constants.AUTHENTICATION.METHODS.TOKEN}";
            string authorizationHeader = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clinetId}:{clientSecret}"))}";
            return await new RequestExecuter().ExecutePostRequestWithErrorChecking<ClientCredentials>(requestUrl, authorizationHeader, keyValuePayload);
        }
    }
}
