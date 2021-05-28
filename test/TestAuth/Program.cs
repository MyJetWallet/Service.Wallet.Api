using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestAuth
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new AuthClient("https://trading-auth-test.mnftx.biz/");

            var login = "alex-003@test.com";
            var passw = "q12345678";

            var rsa = RSA.Create(2048);

            var privateKey = rsa.ExportRSAPrivateKey();
            var publicKey = rsa.ExportRSAPublicKey();

            var authResp = await client.Register(new RegisterContract()
            {
                Email = login,
                Password = passw
            });

            if (authResp.Result != OperationApiResponseCodes.Ok)
            {
                var reg = authResp;
                

                if (authResp.Result == OperationApiResponseCodes.UserExists)
                {
                    authResp = await client.Login(new AuthenticateRequestContract()
                    {
                        Email = login,
                        Password = passw
                    });

                    if (authResp.Result != OperationApiResponseCodes.Ok)
                    {
                        Console.WriteLine("Cannot register client");
                        Console.WriteLine(reg.ToJson());
                        Console.WriteLine();
                        Console.WriteLine("Cannot login client");
                        Console.WriteLine(authResp.ToJson());
                        return;
                    }
                }
            }

            var token = authResp.Data.Token;
            var refreshToken = authResp.Data.RefreshToken;

            Console.WriteLine($"Token: {token}");
            Console.WriteLine();
            Console.WriteLine($"RefreshToken: {refreshToken}");
            Console.WriteLine();

            var refreshResult = await client.RefreshToken(new RefreshTokenContract()
            {
                RefreshToken = refreshToken
            });

            token = refreshResult.Token;
            refreshToken = refreshResult.RefreshToken;

            await client.Logout(new LogoutContract(){Token = token});

            try
            {
                refreshResult = await client.RefreshToken(new RefreshTokenContract()
                {
                    RefreshToken = refreshToken
                });

                token = refreshResult.Token;
                refreshToken = refreshResult.RefreshToken;

                Console.WriteLine("ERROR!!! After login cannot refresh session");
                return;
            }
            catch (Exception)
            {
                Console.WriteLine("+++ After login cannot refresh session");
            }


            authResp = await client.Login(new AuthenticateRequestContract()
            {
                Email = login,
                Password = passw,
                PublicKeyPem = Convert.ToBase64String(publicKey),
                Application = LoginApplication.MobileApp
            });

            if (authResp.Result != OperationApiResponseCodes.Ok)
            {
                Console.WriteLine("Cannot login client");
                Console.WriteLine(authResp.ToJson());
                return;
            }

            token = authResp.Data.Token;
            refreshToken = authResp.Data.RefreshToken;

            Console.WriteLine($"Token: {token}");
            Console.WriteLine();
            Console.WriteLine($"RefreshToken: {refreshToken}");
            Console.WriteLine();


            //var refreshResult = await client.RefreshToken(new RefreshTokenContract()
            //{
            //    RefreshToken = refreshToken
            //});

            var request = new RefreshTokenContract()
            {
                RefreshToken = refreshToken,
                RequestDataTime = DateTime.UtcNow.ToString("O")
            };

            var content = $"{request.RefreshToken}{request.RequestDataTime}";
            var sign = rsa.SignData(Encoding.UTF8.GetBytes(content), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.TokenDateTimeSignatureBase64 = Convert.ToBase64String(sign);


            refreshResult = await client.RefreshToken(request);

            token = refreshResult.Token;
            refreshToken = refreshResult.RefreshToken;

            Console.WriteLine($"Token: {token}");
            Console.WriteLine();
            Console.WriteLine($"RefreshToken: {refreshToken}");
            Console.WriteLine();

            await client.Logout(new LogoutContract() { Token = token });
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

        public async Task<ResponseContract<AuthenticationResponseContract>> Login(AuthenticateRequestContract request)
        {
            var resp = await PostAsync<AuthenticateRequestContract, ResponseContract<AuthenticationResponseContract>>("auth/v1/Trader/Authenticate", request);
            return resp;
        }

        public async Task Logout(LogoutContract request)
        {
            await PostAsync<LogoutContract>("/auth/v1/Trader/Logout", request);
        }

        public async Task<RefreshTokenResponseContract> RefreshToken(RefreshTokenContract request)
        {
            var resp = await PostAsync<RefreshTokenContract, RefreshTokenResponseContract>("auth/v1/Trader/RefreshToken", request);
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

        public async Task PostAsync<TRequest>(string path, TRequest request)
        {
            var resp = await _client.PostAsJsonAsync(path, request);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Cannot execute method, status code: {resp.StatusCode}");
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

    public class LogoutContract
    {
        /// <summary>
        /// Can be ApiToken or RefreshToken
        /// </summary>
        [Required]
        public string Token { get; set; }
    }

    /// <summary>
    /// Authentication contract
    /// </summary>
    public class AuthenticateRequestContract
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

    public class RefreshTokenContract
    {
        [Required]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Request data time in UTC+0. Request will be expired in 2 minutes after this data-time.
        /// Please use controller Common.ServerInfo to get actual server time.
        /// </summary>
        public string RequestDataTime { get; set; }

        /// <summary>
        /// Signature of the refresh command from client private key associated with the session.
        /// Format: Base64 string
        /// Format of signed content: concatenation of RefreshToken and RequestDataTime ("{RefreshToken}{RequestDataTime}").
        /// </summary>
        public string TokenDateTimeSignatureBase64 { get; set; }
    }

    public class RefreshTokenResponseContract
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }
}
