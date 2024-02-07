using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Services
{
    public class SkyflowClient(IOptions<SkyflowOptions> options) : ISkyflowClient
    {
        private const string GrandType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        public async Task<SkyflowBearerTokenResponse> GetBearerToken()
        {
            var privateKey = GetPrivateKey();

            var bytes = Convert.FromBase64String(privateKey);

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(bytes, out _);
            var request = new CertificateRequest("cn=VirtoCommerce", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var notBefore = DateTimeOffset.UtcNow;
            var notAfter = notBefore.AddYears(1);
            var certificate = request.CreateSelfSigned(notBefore, notAfter);
            var signedToken = JwtBuilder.Create()
                .WithAlgorithm(new RS256Algorithm(certificate))
                .AddClaim("iss", options.Value.ClientId)
                .AddClaim("key", options.Value.KeyId)
                .AddClaim("aud", options.Value.TokenUri)
                .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                .AddClaim("sub", options.Value.ClientId)
                .Encode();

            using var httpClient = new HttpClient();
            var payload = new
            {
                grant_type = GrandType,
                assertion = signedToken
            };
            var body = JsonConvert.SerializeObject(payload);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(options.Value.TokenUri, content);
            var responseContent = await response.Content.ReadFromJsonAsync<SkyflowBearerTokenResponse>();
            return responseContent;
        }

        private string GetPrivateKey()
        {
            const string beginKey = "-----BEGIN PRIVATE KEY-----";
            const string endKey = "-----END PRIVATE KEY-----";
            var startIndex = options.Value.PrivateKey.IndexOf(beginKey, StringComparison.InvariantCulture) + beginKey.Length;
            var endIndex = options.Value.PrivateKey.LastIndexOf(endKey, StringComparison.InvariantCulture);
            var result = options.Value.PrivateKey
                .Substring(startIndex, endIndex - startIndex)
                .Replace("\n", "");
            return result;
        }
    }
}
