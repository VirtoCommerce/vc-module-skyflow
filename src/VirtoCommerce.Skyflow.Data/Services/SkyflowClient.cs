using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Services
{
    public class SkyflowClient(IOptions<SkyflowOptions> options, IHttpClientFactory httpClientFactory) : ISkyflowClient
    {
        private const string GrandType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        private readonly SkyflowOptions _options = options.Value;

        public Task<SkyflowBearerTokenResponse> GetBearerToken()
        {
            return GetBearerTokenInternal(_options.ClientSdk);
        }

        public async Task<HttpResponseMessage> InvokeConnection(string connectionName, HttpRequestMessage request)
        {
            if (!_options.Connections.ContainsKey(connectionName))
            {
                throw new ArgumentException($"Connection {connectionName} not found");
            }
            var options = _options.Connections[connectionName];
            var token = await GetBearerTokenInternal(options);
            request.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
            var response = await Send(request);
            return response;
        }

        private async Task<HttpResponseMessage> Send(HttpRequestMessage message)
        {
            using var httpClient = httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(message);
            return response;
        }

        private async Task<SkyflowBearerTokenResponse> GetBearerTokenInternal(SkyflowSdkOptions options)
        {
            var signedToken = GenerateToken(options);

            var payload = new { grant_type = GrandType, assertion = signedToken };
            var body = JsonConvert.SerializeObject(payload);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUri)
            {
                Content = content
            };

            var response = await Send(request);
            var responseContent = await response.Content.ReadFromJsonAsync<SkyflowBearerTokenResponse>();

            if (responseContent == null || string.IsNullOrEmpty(responseContent.AccessToken))
            {
                throw new InvalidOperationException("Failed to get bearer token");
            }
            return responseContent;
        }

        private string GenerateToken(SkyflowSdkOptions options)
        {
            if (string.IsNullOrEmpty(options.KeyId))
            {
                throw new ArgumentNullException(nameof(options.KeyId));
            }

            if (string.IsNullOrEmpty(options.ClientId))
            {
                throw new ArgumentNullException(nameof(options.ClientId));
            }

            if (string.IsNullOrEmpty(_options.TokenUri))
            {
                throw new ArgumentNullException(nameof(_options.TokenUri));
            }

            var certificate = CreateCertificate(options);
            var builder = JwtBuilder.Create()
                .WithAlgorithm(new RS256Algorithm(certificate))
                .AddClaim("iss", options.ClientId)
                .AddClaim("key", options.KeyId)
                .AddClaim("aud", _options.TokenUri)
                .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                .AddClaim("sub", options.ClientId);
            var signedToken = builder.Encode();
            return signedToken;
        }

        private static X509Certificate2 CreateCertificate(SkyflowSdkOptions options)
        {
            var privateKey = GetPrivateKey(options);

            var bytes = Convert.FromBase64String(privateKey);

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(bytes, out _);
            var request = new CertificateRequest("cn=VirtoCommerce", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var notBefore = DateTimeOffset.UtcNow;
            var notAfter = notBefore.AddYears(1);
            var certificate = request.CreateSelfSigned(notBefore, notAfter);
            return certificate;
        }

        private static string GetPrivateKey(SkyflowSdkOptions options)
        {
            const string beginKey = "-----BEGIN PRIVATE KEY-----";
            const string endKey = "-----END PRIVATE KEY-----";
            var startIndex = options.PrivateKey.IndexOf(beginKey, StringComparison.InvariantCulture) + beginKey.Length;
            var endIndex = options.PrivateKey.LastIndexOf(endKey, StringComparison.InvariantCulture);

            if (startIndex < beginKey.Length || endIndex < beginKey.Length)
            {
                throw new InvalidOperationException("Wrong PrivateKey setting");
            }

            var result = options.PrivateKey[startIndex..endIndex]
                .Replace("\n", "").Replace("\\n", "").Replace(" ", "");
            return result;
        }

#if DEBUG

        private string Decode(SkyflowSdkOptions options, string jwt)
        {
            var certificate = CreateCertificate(options);
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new RS256Algorithm(certificate);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            var json = decoder.Decode(jwt);
            return json;
        }

        private string GenerateTokenStandard(SkyflowSdkOptions options)
        {
            // way to get token using standard library

            var privateKey = GetPrivateKey(options);
            var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

            var claims = new Claim[]
            {
                new(JwtRegisteredClaimNames.Iss, options.ClientId),
                new("key", options.KeyId),
                new(JwtRegisteredClaimNames.Aud, _options.TokenUri),
                new(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Sub, options.ClientId)
            };

            var token = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);

            var handler = new JwtSecurityTokenHandler();
            var tokenString = handler.WriteToken(token);
            return tokenString;
        }
#endif
    }
}
