using Ecomerce.Models;

namespace Ecomerce.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model); 
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
