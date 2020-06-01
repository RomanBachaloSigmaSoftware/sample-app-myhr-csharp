using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.MyHR.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DocuSign.MyHR.Security
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configurationService;
        private ApiClient _apiClient;

        public AuthenticationService(IConfiguration configurationService)
        {
            _configurationService = configurationService;
            _apiClient = new ApiClient();
            _apiClient.SetOAuthBasePath(_configurationService["DocuSign:AuthServer"]);
        }

        public (ClaimsPrincipal, AuthenticationProperties) AuthenticateFromJwt()
        {
            OAuth.OAuthToken authToken = _apiClient.RequestJWTUserToken(
                ConfigurationManager.AppSettings["IntegrationKey"],
                ConfigurationManager.AppSettings["UserId"],
                ConfigurationManager.AppSettings["AuthServer"],
                Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["RSAKey"]),
                1,
                new List<string> { "click.manage", "signature" });

            OAuth.UserInfo userInfo = _apiClient.GetUserInfo(authToken.access_token);
            var claims = new List<Claim>
            {
                new Claim("access_token", authToken.access_token),
                new Claim(ClaimTypes.NameIdentifier, userInfo.Sub),
                new Claim(ClaimTypes.Name, userInfo.Name),
                new Claim("account_id",  userInfo.Accounts.First(x => x.IsDefault == "true").AccountId),
                new Claim("authType", LoginType.JWT.ToString()),
            };

            foreach (var account in userInfo.Accounts)
            {
                claims.Add(new Claim("accounts", JsonConvert.SerializeObject(account)));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                IsPersistent = true,
            };

            return (new ClaimsPrincipal(claimsIdentity), authProperties);
        }
    }
}