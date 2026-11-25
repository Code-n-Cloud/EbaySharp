using EbaySharp.Entities.Identity;
using EbaySharp.Source;
using System.Net;

namespace EbaySharp.Tests
{
    // Offline tests for the error handling of the OAuth token request (no eBay credentials required).
    public class RequestExecuterTests
    {
        private const string TOKEN_URL = "https://api.ebay.com/identity/v1/oauth2/token";

        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode statusCode;
            private readonly string responseBody;

            public StubHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
            {
                this.statusCode = statusCode;
                this.responseBody = responseBody;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(responseBody) });
            }
        }

        private static List<KeyValuePair<string, string>> GetTokenPayload()
        {
            return
            [
                new("grant_type", "refresh_token"),
                new("refresh_token", "dummy"),
                new("scope", "https://api.ebay.com/oauth/api_scope")
            ];
        }

        [Test]
        public void ExecutePostRequestWithErrorChecking_ThrowsWithEbayErrorBody()
        {
            const string errorBody = "{\"error\":\"invalid_grant\",\"error_description\":\"the provided authorization grant code is invalid or was issued to another client\"}";
            var requestExecuter = new RequestExecuter(new StubHttpMessageHandler(HttpStatusCode.BadRequest, errorBody));

            var exception = Assert.ThrowsAsync<Exception>(() =>
                requestExecuter.ExecutePostRequestWithErrorChecking<ClientCredentials>(TOKEN_URL, "Basic ZHVtbXk=", GetTokenPayload()));

            Assert.That(exception!.Message, Does.Contain("invalid_grant"));
        }

        [Test]
        public void ExecutePostRequestWithErrorChecking_ThrowsWithStatusWhenErrorBodyIsEmpty()
        {
            var requestExecuter = new RequestExecuter(new StubHttpMessageHandler(HttpStatusCode.Unauthorized, ""));

            var exception = Assert.ThrowsAsync<Exception>(() =>
                requestExecuter.ExecutePostRequestWithErrorChecking<ClientCredentials>(TOKEN_URL, "Basic ZHVtbXk=", GetTokenPayload()));

            Assert.That(exception!.Message, Does.Contain("Unauthorized"));
        }

        [Test]
        public void ExecutePostRequestWithErrorChecking_ThrowsWhenSuccessBodyIsEmpty()
        {
            var requestExecuter = new RequestExecuter(new StubHttpMessageHandler(HttpStatusCode.OK, ""));

            var exception = Assert.ThrowsAsync<Exception>(() =>
                requestExecuter.ExecutePostRequestWithErrorChecking<ClientCredentials>(TOKEN_URL, "Basic ZHVtbXk=", GetTokenPayload()));

            Assert.That(exception!.Message, Is.EqualTo("No content found."));
        }

        [Test]
        public async Task ExecutePostRequestWithErrorChecking_ReturnsCredentialsOnSuccess()
        {
            var requestExecuter = new RequestExecuter(new StubHttpMessageHandler(HttpStatusCode.OK,
                "{\"access_token\":\"v^1.1#i^1#token\",\"expires_in\":7200,\"token_type\":\"User Access Token\"}"));

            ClientCredentials clientCredentials = await requestExecuter.ExecutePostRequestWithErrorChecking<ClientCredentials>(TOKEN_URL, "Basic ZHVtbXk=", GetTokenPayload());

            Assert.Multiple(() =>
            {
                Assert.That(clientCredentials.AccessToken, Is.EqualTo("v^1.1#i^1#token"));
                Assert.That(clientCredentials.ExpiresIn, Is.EqualTo(7200));
            });
        }

        [Test]
        public async Task ExecutePostRequest_StillReturnsDefaultOnErrorForBackwardCompatibility()
        {
            var requestExecuter = new RequestExecuter(new StubHttpMessageHandler(HttpStatusCode.BadRequest, ""));

            ClientCredentials clientCredentials = await requestExecuter.ExecutePostRequest<ClientCredentials>(TOKEN_URL, "Basic ZHVtbXk=", GetTokenPayload());

            Assert.That(clientCredentials, Is.Null);
        }
    }
}
