using System.Security.Cryptography;
using System.Text;
using Framework_Project.Models;
using Framework_Project.Models.Momo;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace Framework_Project.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }
        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfo model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (string.IsNullOrEmpty(_options.Value.PartnerCode) || 
                string.IsNullOrEmpty(_options.Value.AccessKey) || 
                string.IsNullOrEmpty(_options.Value.SecretKey))
            {
                throw new InvalidOperationException("MoMo configuration is incomplete");
            }

            model.OrderId = DateTime.UtcNow.Ticks.ToString();
            model.OrderInformation = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInformation;
            
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}" +
                $"&accessKey={_options.Value.AccessKey}" +
                $"&requestId={model.OrderId}" +
                $"&amount={((long)model.Amount).ToString()}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderInformation}" +
                $"&returnUrl={_options.Value.ReturnUrl}" +
                $"&notifyUrl={_options.Value.NotifyUrl}" +
                $"&extraData=";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            var requestData = new
            {
                partnerCode = _options.Value.PartnerCode,
                accessKey = _options.Value.AccessKey,
                requestId = model.OrderId,
                amount = ((long)model.Amount).ToString(),
                orderId = model.OrderId,
                orderInfo = model.OrderInformation,
                returnUrl = _options.Value.ReturnUrl,
                notifyUrl = _options.Value.NotifyUrl,
                extraData = "",
                requestType = _options.Value.RequestType,
                signature = signature
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);
            
            try
            {
                var response = await client.ExecuteAsync(request);
                
                // Debug: Save raw response to file for troubleshooting
                System.IO.File.WriteAllText("momo_response_debug.txt", response.Content);
                
                if (!response.IsSuccessful)
                {
                    throw new Exception($"MoMo API call failed: {response.ErrorMessage}");
                }

                var result = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
                
                if (result == null)
                {
                    throw new Exception("Failed to deserialize MoMo response");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating MoMo payment: {ex.Message}", ex);
            }
        }
        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;
            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashString;
        }
    }
}
