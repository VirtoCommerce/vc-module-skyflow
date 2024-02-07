using System;
using System.Collections.Generic;
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
    public class SkyflowClient(IOptions<SkyflowOptions> options) : ISkyflowClient
    {
        private const string GrandType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        public async Task<Dictionary<string, string>> GetBearerToken()
        {
            var result = new Dictionary<string, string>();
            result.Add("pk", options.Value.PrivateKey);
            result.Add("pkс", GetPrivateKey());
            result.Add("cid", options.Value.ClientId);
            result.Add("kid", options.Value.KeyId);
            result.Add("uri", options.Value.TokenUri);
            result.Add("name", options.Value.ClientName);
            try
            {
                var signedToken = GenerateToken();

                using var httpClient = new HttpClient();
                var payload = new { grant_type = GrandType, assertion = signedToken };
                var body = JsonConvert.SerializeObject(payload);
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(options.Value.TokenUri, content);

                var responseString = await response.Content.ReadAsStringAsync();
                result.Add("response", responseString);

                var responseContent = await response.Content.ReadFromJsonAsync<SkyflowBearerTokenResponse>();
                result.Add("token", responseContent.AccessToken);
            }
            catch (Exception e)
            {
                result.Add("error", e.ToString());
            }

            return result;
        }


        private string GenerateToken()
        {
            var certificate = CreateCertificate();
            var builder = JwtBuilder.Create()
                .WithAlgorithm(new RS256Algorithm(certificate))
                .AddClaim("iss", options.Value.ClientId)
                .AddClaim("key", options.Value.KeyId)
                .AddClaim("aud", options.Value.TokenUri)
                .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                .AddClaim("sub", options.Value.ClientId);
            var signedToken = builder.Encode();
            return signedToken;
        }

        private X509Certificate2 CreateCertificate()
        {
            var privateKey = GetPrivateKey();

            var bytes = Convert.FromBase64String(privateKey);

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(bytes, out _);
            var request = new CertificateRequest("cn=VirtoCommerce", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var notBefore = DateTimeOffset.UtcNow;
            var notAfter = notBefore.AddYears(1);
            var certificate = request.CreateSelfSigned(notBefore, notAfter);
            return certificate;
        }

        private string GetPrivateKey()
        {
            const string beginKey = "-----BEGIN PRIVATE KEY-----";
            const string endKey = "-----END PRIVATE KEY-----";
            var startIndex = options.Value.PrivateKey.IndexOf(beginKey, StringComparison.InvariantCulture) + beginKey.Length;
            var endIndex = options.Value.PrivateKey.LastIndexOf(endKey, StringComparison.InvariantCulture);
            var result = options.Value.PrivateKey[startIndex..endIndex]
                .Replace("\n", "").Replace("\\n", "").Replace(" ", "");
            return result;
        }

#if DEBUG

        private string Decode(string jwt)
        {
            var certificate = CreateCertificate();
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new RS256Algorithm(certificate);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            var json = decoder.Decode(jwt);
            return json;
        }

        private string GenerateTokenStandard()
        {
            // way to get token using standard library

            var privateKey = GetPrivateKey();
            var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

            var claims = new Claim[]
            {
                new(JwtRegisteredClaimNames.Iss, options.Value.ClientId),
                new("key", options.Value.KeyId),
                new(JwtRegisteredClaimNames.Aud, options.Value.TokenUri),
                new(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Sub, options.Value.ClientId)
            };

            var token = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);

            var handler = new JwtSecurityTokenHandler();
            var tokenString = handler.WriteToken(token);
            return tokenString;
        }
#endif
    }
}
