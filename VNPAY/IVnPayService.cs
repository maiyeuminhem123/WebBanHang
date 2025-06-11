using Bai3_WebBanHang.Models;
using Microsoft.AspNetCore.Http;
namespace Bai3_WebBanHang.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}