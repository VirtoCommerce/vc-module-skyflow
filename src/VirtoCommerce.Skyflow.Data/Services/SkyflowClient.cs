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
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Services;

public class SkyflowClient(
        IOptions<SkyflowOptions> options,
        IHttpClientFactory httpClientFactory) : ISkyflowClient
{
    private const string GrandType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
    private readonly SkyflowOptions _options = options.Value;


    public Task<SkyflowBearerTokenResponse> GetBearerToken(SkyflowServiceAccountOptions serviceAccount)
    {
        return GetBearerTokenInternal(serviceAccount);
    }


    public async Task<IEnumerable<SkyflowCard>> GetAllSavedUserCards(string userId)
    {
        // required the Vault Viewer permission
        if (userId is null or "Anonymous")
        {
            return Array.Empty<SkyflowCard>();
        }
        var url = $"{_options.VaultUri}/v1/vaults/{_options.VaultId}/query";
        var body = JsonConvert.SerializeObject(new { _options.TableName, query = $"SELECT * FROM {_options.TableName} WHERE user_id = '{userId}'" });
        var result = await GetSkyflowResponse<SkyflowResponseModel>(HttpMethod.Post, url, body);

        return result.Records.Select(x => x.Fields);
    }

    public async Task<bool> DeleteCard(string skyflowId)
    {
        // required the Vault Owner permission
        var url = $"{_options.VaultUri}/v1/vaults/{_options.VaultId}/{_options.TableName}/{skyflowId}";
        var result = await GetSkyflowResponse<SkyflowDeleteResponseModel>(HttpMethod.Delete, url);
        return result.Deleted;
    }

    public async Task<HttpResponseMessage> InvokeConnection(HttpMethod method, string route, Dictionary<string, string> headers, HttpContent content)
    {
        ArgumentNullException.ThrowIfNull(route);
        route = route.TrimStart('/');
        var request = new HttpRequestMessage(method, new Uri($"{_options.GatewayUri}/v1/gateway/outboundRoutes/{route}"))
        {
            Content = content
        };

        var token = await GetBearerTokenInternal(_options.IntegrationsAccount);
        request.Headers.Add("X-Skyflow-Authorization", $"{token.AccessToken}");
        request.Headers.Add("User-Agent", "Opus/1.0");
        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }
        var response = await Send(request);
        return response;
    }

    public async Task<SkyflowCard> GetCard(string skyflowId)
    {
        // required the Vault Owner permission
        // https://ebfc9bee4242.vault.skyflowapis.com/v1/vaults/c1aeec61ad7c46c2b724f004a7658b2f/credit_cards/5fa1d8e1-9b9d-4eb7-905d-31df6e93cf7e?tokenization=true
        // get tokenized data
        var tokenUrl = $"{_options.VaultUri}/v1/vaults/{_options.VaultId}/{_options.TableName}/{skyflowId}?tokenization=true";
        var tokenResult = await GetSkyflowResponse<SkyflowTableRowModel>(HttpMethod.Get, tokenUrl);
        tokenResult.Fields = tokenResult.Fields.WithDefaultValue(null);

        var result = new SkyflowCard
        {
            SkyflowId = skyflowId,
            CardNumber = tokenResult.Fields["card_number"],
            CardExpiration = tokenResult.Fields["card_expiration"],
            Cvv = tokenResult.Fields["cvv"],
            CardholderName = tokenResult.Fields["cardholder_name"],
            UserId = tokenResult.Fields["user_id"]
        };
        return result;
    }

    public async Task<SkyflowCard[]> GetCardsByIds(string[] skyflowIds)
    {
        // required the Vault Owner permission
        // https://ebfc9bee4242.vault.skyflowapis.com/v1/vaults/c1aeec61ad7c46c2b724f004a7658b2f/credit_cards/5fa1d8e1-9b9d-4eb7-905d-31df6e93cf7e?tokenization=true
        // get tokenized data
        var tokenUrl = $"{_options.VaultUri}/v1/vaults/{_options.VaultId}/{_options.TableName}?" + string.Join("&", skyflowIds.Select(x => $"skyflow_ids={x}"));
        var response = await GetSkyflowResponse<SkyflowResponseModel>(HttpMethod.Get, tokenUrl);
        return response?.Records.Select(x => x.Fields).ToArray();
    }

    private async Task<T> GetSkyflowResponse<T>(HttpMethod method, string url, string body = null)
    {
        var token = await GetBearerTokenInternal(_options.IntegrationsAccount);
        var request = new HttpRequestMessage(method, url);
        if (!body.IsNullOrEmpty())
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }
        request.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
        var response = await Send(request);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(error);
        }
        var result = await response.Content.ReadFromJsonAsync<T>();
        return result;
    }

    private async Task<HttpResponseMessage> Send(HttpRequestMessage message)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(message);
        return response;
    }

    private async Task<SkyflowBearerTokenResponse> GetBearerTokenInternal(SkyflowServiceAccountOptions serviceAccount)
    {
        var signedToken = GenerateToken(serviceAccount);

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

    private string GenerateToken(SkyflowServiceAccountOptions serviceAccount)
    {
        var certificate = CreateCertificate(serviceAccount);
        var builder = JwtBuilder.Create()
            .WithAlgorithm(new RS256Algorithm(certificate))
            .AddClaim("iss", serviceAccount.ClientId)
            .AddClaim("key", serviceAccount.KeyId)
            .AddClaim("aud", _options.TokenUri)
            .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
            .AddClaim("sub", serviceAccount.ClientId);
        var signedToken = builder.Encode();
        return signedToken;
    }

    private static X509Certificate2 CreateCertificate(SkyflowServiceAccountOptions serviceAccount)
    {
        var privateKey = GetPrivateKey(serviceAccount);

        var bytes = Convert.FromBase64String(privateKey);

        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(bytes, out _);
        var request = new CertificateRequest("cn=VirtoCommerce", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var notBefore = DateTimeOffset.UtcNow;
        var notAfter = notBefore.AddYears(1);
        var certificate = request.CreateSelfSigned(notBefore, notAfter);
        return certificate;
    }

    private static string GetPrivateKey(SkyflowServiceAccountOptions serviceAccount)
    {
        const string beginKey = "-----BEGIN PRIVATE KEY-----";
        const string endKey = "-----END PRIVATE KEY-----";
        var startIndex = serviceAccount.PrivateKey.IndexOf(beginKey, StringComparison.InvariantCulture) + beginKey.Length;
        var endIndex = serviceAccount.PrivateKey.LastIndexOf(endKey, StringComparison.InvariantCulture);

        if (startIndex < beginKey.Length || endIndex < beginKey.Length)
        {
            throw new InvalidOperationException("Wrong PrivateKey setting");
        }

        var result = serviceAccount.PrivateKey[startIndex..endIndex]
            .Replace("\n", "").Replace("\\n", "").Replace(" ", "");
        return result;
    }

}
