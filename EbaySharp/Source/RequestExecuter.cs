using EbaySharp.Entities.Develop.KeyManagement.SigningKey;
using EbaySharp.Entities.Develop.SellingApps.ListingManagement.Feed.Task;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace EbaySharp.Source
{
    internal class RequestExecuter
    {
        private async Task<HttpResponseMessage> executeRequest(HttpMethod httpMethod, string requestUrl, string authenticationHeaderValue, string contentLanguageHeaderValue,
            List<KeyValuePair<string, string>> keyValuePayload, string JSONPayload, SigningKey signingKey)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(httpMethod, requestUrl);
            request.Headers.Add("Authorization", authenticationHeaderValue);

            if (keyValuePayload != null)
            {
                var content = new FormUrlEncodedContent(keyValuePayload);
                request.Content = content;
            }
            else if (signingKey != null)
            {
                request.GenerateEbaySignatureHeader(httpMethod, requestUrl, null, signingKey);
            }
            else if (JSONPayload != null)
            {
                var content = new StringContent(JSONPayload, null, "application/json");
                if (string.IsNullOrEmpty(contentLanguageHeaderValue) == false)
                {
                    content.Headers.Add("Content-Language", contentLanguageHeaderValue.Replace("_", "-"));
                }
                request.Content = content;
            }
            var response = await client.SendAsync(request);
            return response;
        }
        public async Task<T> ExecuteGetRequest<T>(string requestUrl, string authenticationHeaderValue)
        {
            return await ExecuteGetRequest<T>(requestUrl, authenticationHeaderValue, null);
        }
        public async Task<T> ExecuteGetRequest<T>(string requestUrl, string authenticationHeaderValue, SigningKey signingKey)
        {
            HttpResponseMessage response = await executeRequest(HttpMethod.Get, requestUrl, authenticationHeaderValue, null, null, null, signingKey);

            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                MediaTypeHeaderValue contentType = response.Content.Headers.ContentType;
                if (contentType != null && contentType.MediaType == "application/octet-stream")
                {
                    string contentDisposition = response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault();
                    if (contentDisposition != null)
                    {
                        string fileName = contentDisposition.Split(";", StringSplitOptions.RemoveEmptyEntries)[1].Split("=", StringSplitOptions.RemoveEmptyEntries)[1];
                        if (contentDisposition != null)
                        {
                            ResultFile resultFile = new ResultFile() { FileContent = await response.Content.ReadAsStreamAsync(), FileName = fileName.Trim() };
                            return (T)Convert.ChangeType(resultFile, typeof(T));
                        }
                    }

                }
                if (string.IsNullOrEmpty(responseContent))
                {
                    throw new Exception("No content found.");
                }
                return responseContent.DeserializeToObject<T>();
            }
            else if (string.IsNullOrEmpty(responseContent))
            {
                responseContent = JsonSerializer.Serialize(new { Reason = response.ReasonPhrase, StatusCode = response.StatusCode });
            }
            throw new Exception(responseContent);
        }
        public async Task ExecuteDeleteRequest(string requestUrl, string authenticationHeaderValue)
        {
            HttpResponseMessage response = await executeRequest(HttpMethod.Delete, requestUrl, authenticationHeaderValue, null, null, null, null);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false)
            {
                throw new Exception(responseContent);
            }
        }
        // Sends a request and returns the body deserialized into T. It never throws: both success bodies
        // and error bodies (which carry eBay's warnings/errors) are returned on the object, and an empty
        // or non-JSON body yields default(T).
        private async Task<T> executeJsonRequest<T>(HttpMethod httpMethod, string requestUrl, string authenticationHeaderValue,
            string contentLanguage, List<KeyValuePair<string, string>> keyValuePayload, string JSONPayload)
        {
            HttpResponseMessage response = await executeRequest(httpMethod, requestUrl, authenticationHeaderValue, contentLanguage, keyValuePayload, JSONPayload, null);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseContent))
            {
                return default(T);
            }
            try
            {
                return responseContent.DeserializeToObject<T>();
            }
            catch (JsonException)
            {
                return default(T);
            }
        }
        private async Task<T> executeLegacyPostRequest<T>(int siteId, string callName, string payload, int compatibilityLevel, SigningKey signingKey)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60 * 3);
            string requestUrl = $"{Constants.API_SERVER_URL}{Constants.TRADIONAL.ENDPOINT_URL}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Add("X-EBAY-API-SITEID", siteId.ToString());
            request.Headers.Add("X-EBAY-API-COMPATIBILITY-LEVEL", compatibilityLevel.ToString());
            request.Headers.Add("X-EBAY-API-CALL-NAME", callName);
            if (signingKey != null)
            {
                request.GenerateEbaySignatureHeader(HttpMethod.Post, requestUrl, payload, signingKey);
            }
            var content = new StringContent(payload, null, "application/xml");
            request.Content = content;
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            XmlSerializer ser = new XmlSerializer(typeof(T));
            T type;
            using (XmlReader reader = XmlReader.Create(new StringReader(responseContent)))
            {
                type = (T)ser.Deserialize(reader);
            }
            return type;
        }
        public async Task<T> ExecutePutRequest<T>(string requestUrl, string authenticationHeaderValue, string JSONPayload, string contentLanguage)
        {
            return await executeJsonRequest<T>(HttpMethod.Put, requestUrl, authenticationHeaderValue, contentLanguage, null, JSONPayload);
        }
        public async Task<T> ExecutePostRequest<T>(string requestUrl, string authenticationHeaderValue, List<KeyValuePair<string, string>> keyValuePayload)
        {
            return await executeJsonRequest<T>(HttpMethod.Post, requestUrl, authenticationHeaderValue, null, keyValuePayload, null);
        }
        public async Task<T> ExecuteLegacyPostRequest<T>(int siteId, string callName, string payload, int compatibilityLevel, SigningKey signingKey)
        {
            return await executeLegacyPostRequest<T>(siteId, callName, payload, compatibilityLevel, signingKey);
        }
        public async Task<T> ExecutePostRequest<T>(string requestUrl, string authenticationHeaderValue, string JSONPayload, string contentLanguage)
        {
            return await executeJsonRequest<T>(HttpMethod.Post, requestUrl, authenticationHeaderValue, contentLanguage, null, JSONPayload);
        }
        public async Task ExecutePostRequest(string requestUrl, string authenticationHeaderValue, string JSONPayload, string contentLanguage)
        {
            // No response object to return, so a failed request is not surfaced here; use the generic
            // ExecutePostRequest<T> when warnings/errors need to be inspected.
            await ExecutePostRequest<object>(requestUrl, authenticationHeaderValue, JSONPayload, contentLanguage);
        }
        public async Task<T> ExecutePostRequest<T>(string requestUrl, string authenticationHeaderValue, string JSONPayload)
        {
            return await ExecutePostRequest<T>(requestUrl, authenticationHeaderValue, JSONPayload, null);
        }
        public async Task ExecutePostRequest(string requestUrl, string authenticationHeaderValue, string JSONPayload)
        {
            await ExecutePostRequest(requestUrl, authenticationHeaderValue, JSONPayload, null);
        }
    }
}
