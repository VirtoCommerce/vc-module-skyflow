using System;
using System.Collections.Generic;
using System.Linq;
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
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Skyflow.Core;
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

        public Task<HttpResponseMessage> InvokeConnection(string connectionName, HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!_options.Connections.TryGetValue(connectionName, out var connectionOptions))
            {
                throw new ArgumentException($"Connection {connectionName} not found");
            }

            return InvokeConnectionInternal(connectionOptions, request);
        }

        public async Task<IEnumerable<SkyflowCard>> GetCards(SkyflowStoreConfig config, string userId)
        {
            // required the Vault Viewer permission
            if (userId == null || userId == "Anonymous")
            {
                return Array.Empty<SkyflowCard>();
            }

            var url = $"{config.VaultUrl.TrimEnd('/')}/v1/vaults/{config.VaultId}/query";
            var body = JsonConvert.SerializeObject(new { config.TableName, query = $"SELECT * FROM {config.TableName} WHERE user_id = '{userId}'" });
            var result = await GetSkyflowResponse<SkyflowResponseModel>(HttpMethod.Post, url, ModuleConstants.VaultViewerRoleConfigName, body);

            return result.Records.Select(x => x.Fields);
        }

        public async Task<IDictionary<string, string>> GetCardTokens(SkyflowStoreConfig config, string skyflowId, string userId)
        {
            // required the Vault Owner permission
            var url = $"{config.VaultUrl.TrimEnd('/')}/v1/vaults/{config.VaultId}/{config.TableName}/{skyflowId}";
            var result = await GetSkyflowResponse<SkyflowTableRowModel>(HttpMethod.Get, url, ModuleConstants.VaultOwnerRoleConfigName);
            return result.Fields;
        }

        public async Task<bool> DeleteCard(SkyflowStoreConfig config, string skyflowId)
        {
            // required the Vault Owner permission
            var url = $"{config.VaultUrl.TrimEnd('/')}/v1/vaults/{config.VaultId}/{config.TableName}/{skyflowId}";
            var result = await GetSkyflowResponse<SkyflowDeleteResponseModel>(HttpMethod.Delete, url, ModuleConstants.VaultOwnerRoleConfigName);
            return result.Deleted;
        }

        private async Task<T> GetSkyflowResponse<T>(HttpMethod method, string url, string connectionName, string body = null)
        {
            var connectionOptions = _options.Connections.TryGetValue(connectionName, out var connection)
                ? connection
                : _options.Connections["Default"];

            var token = await GetBearerTokenInternal(connectionOptions);
            var request = new HttpRequestMessage(method, url);
            if (!body.IsNullOrEmpty())
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }
            request.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
            var response = await Send(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(error);
            }
            var result = await response.Content.ReadFromJsonAsync<T>();
            return result;
        }

        private async Task<HttpResponseMessage> InvokeConnectionInternal(SkyflowSdkOptions options, HttpRequestMessage request)
        {
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
                throw new PlatformException($"{nameof(options.KeyId)} must be set");
            }

            if (string.IsNullOrEmpty(options.ClientId))
            {
                throw new PlatformException($"{nameof(options.ClientId)} must be set");
            }

            if (string.IsNullOrEmpty(_options.TokenUri))
            {
                throw new PlatformException($"{nameof(_options.TokenUri)} must be set");
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

        //private string Decode(SkyflowSdkOptions options, string jwt)
        //{
        //    var certificate = CreateCertificate(options);
        //    IJsonSerializer serializer = new JsonNetSerializer();
        //    IDateTimeProvider provider = new UtcDateTimeProvider();
        //    IJwtValidator validator = new JwtValidator(serializer, provider);
        //    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        //    IJwtAlgorithm algorithm = new RS256Algorithm(certificate);
        //    IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

        //    var json = decoder.Decode(jwt);
        //    return json;
        //}

        //private string GenerateTokenStandard(SkyflowSdkOptions options)
        //{
        //    // way to get token using standard library

        //    var privateKey = GetPrivateKey(options);
        //    var rsa = RSA.Create();
        //    rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

        //    var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        //    var claims = new Claim[]
        //    {
        //        new(JwtRegisteredClaimNames.Iss, options.ClientId),
        //        new("key", options.KeyId),
        //        new(JwtRegisteredClaimNames.Aud, _options.TokenUri),
        //        new(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString()),
        //        new(JwtRegisteredClaimNames.Sub, options.ClientId)
        //    };

        //    var token = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);

        //    var handler = new JwtSecurityTokenHandler();
        //    var tokenString = handler.WriteToken(token);
        //    return tokenString;
        //}
    }
}
