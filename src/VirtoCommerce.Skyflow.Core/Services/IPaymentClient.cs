using System.Net.Http;
using VirtoCommerce.PaymentModule.Model.Requests;

namespace VirtoCommerce.Skyflow.Core.Services
{
    public interface IPaymentClient
    {
        HttpRequestMessage CreateConnectionRequest(PaymentRequestBase request);
        PostProcessPaymentRequestResult CreatePostProcessPaymentResponse(PaymentRequestBase request, HttpResponseMessage responseMessage);
        string[] RequiredParameters { get; }
    }
}
