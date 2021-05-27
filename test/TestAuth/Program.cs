using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestAuth
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new AuthClient("https://trading-auth-test.mnftx.biz/");

            var regResp = await client.Register(new RegisterContract()
            {
                Email = "alex-002@test.com",
                Password = "q12345678"
            });

            if (regResp.Result != OperationApiResponseCodes.Ok)
            {
                Console.WriteLine("Cannot register client");
                Console.WriteLine(regResp.ToJson());
                return;
            }

            var token = regResp.Data.Token;
            var refreshToken = regResp.Data.RefreshToken;

            Console.WriteLine($"Token: {token}");
            Console.WriteLine($"RefreshToken: {refreshToken}");



        }
    }


    

    public class AuthClient
    {
        private HttpClient _client;

        public AuthClient(string url)
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(url)
            };
        }

        public async Task<ResponseContract<AuthenticationResponseContract>> Register(RegisterContract request)
        {
            var resp = await PostAsync<RegisterContract, ResponseContract<AuthenticationResponseContract>>("auth/v1/Trader/Register", request);
            return resp;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request)
        {
            var resp = await _client.PostAsJsonAsync(path, request);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Cannot execute method, status code: {resp.StatusCode}");

            var json = await resp.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<TResponse>(json);

            return response;
        }

        public void AddToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void RemoveToken()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }
    }

    public static class Helper
    {
        public static string ToJson(this object data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
    }



    public class RegisterContract
    {
        /// <summary>
        /// Username
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Captcha for web application
        /// </summary>
        public string? Captcha { get; set; }

        public string? Phone { get; set; }

        /// <summary>
        /// Client public key to sign request into the session
        /// </summary>
        public string? PublicKeyPem { get; set; }

        /// <summary>
        /// Application where user want to make a session
        /// Base on the application , API will decide to bind the session to IP address
        /// By default: WebApp
        /// </summary>
        public LoginApplication Application { get; set; } = LoginApplication.WebApp;
    }

    /// <summary>
    /// Application where user want make a session
    /// </summary>
    public enum LoginApplication
    {
        WebApp,
        MobileApp
    }

    public class ResponseContract
    {
        /// <summary>
        /// Operation result
        /// </summary>
        [JsonProperty("result")]
        public OperationApiResponseCodes Result { get; set; }

        public static ResponseContract Create(OperationApiResponseCodes src)
        {
            return new ResponseContract { Result = src };
        }
    }

    public class ResponseContract<T> : ResponseContract
    {
        /// <summary>
        /// Domain object
        /// </summary>
        [JsonProperty("data")]
        public T Data { get; set; }
    }

    /// <summary>
    /// Authentication response
    /// </summary>
    public class AuthenticationResponseContract
    {
        /// <summary>
        /// Auth Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Refresh Token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// ApplicationUrl
        /// </summary>
        public string TradingUrl { get; set; }

        /// <summary>
        /// Connection SignalR TimeOut
        /// </summary>
        public string ConnectionTimeOut { get; set; }

        /// <summary>
        /// Reconnect SignalR TimeOut
        /// </summary>
        public string ReconnectTimeOut { get; set; }
    }

    public enum OperationApiResponseCodes
    {
        /// <summary>
        /// ForceUpdate for native applications
        /// </summary>
        ForceUpdate = -999,

        /// <summary>
        /// Expired
        /// </summary>
        Expired = -9,

        /// <summary>
        /// PersonalData Not Valid
        /// </summary>
        SystemError = -8,

        /// <summary>
        /// PersonalData Not Valid
        /// </summary>
        PersonalDataNotValid = -7,

        /// <summary>
        /// File Not Found
        /// </summary>
        FileNotFound = -6,

        /// <summary>
        /// File Wrong Extension
        /// </summary>
        FileWrongExtension = -5,

        /// <summary>
        /// Old password Not Match
        /// </summary>
        OldPasswordNotMatch = -4,

        /// <summary>
        /// User not exist
        /// </summary>
        UserNotExist = -3,

        /// <summary>
        /// User already exists
        /// </summary>
        UserExists = -2,
        /// <summary>
        /// Invalid username or password
        /// </summary>
        InvalidUserNameOrPassword = -1,

        /// <summary>
        /// Ok
        /// </summary>
        Ok
    }
}
