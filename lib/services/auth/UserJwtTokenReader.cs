using lib.models.db;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System.Linq;
using System.Collections.Generic;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using lib.models.configuration;
using System.Text.Json;
using lib.services;

namespace lib.services.auth
{
    public interface IUserJwtTokenReader
    {
        Task<User?> GetUserFromJwtAsync(string jwtToken);
        Task<ClaimsPrincipal> GetClaimsPrincipalFromJwtAsync(string jwtToken);
        Task<List<string>> GetEmailsFromJwtAsync(string jwtToken);
        Task<string> GetJwtSubjectFromJwtAsync(string jwtToken);
    }

    public class UserJwtTokenReader : IUserJwtTokenReader
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _oidcConfigMgr;
        private readonly string _userIdJwtClaim;

        public UserJwtTokenReader(
            IServiceScopeFactory scopeFactory, 
            ILogger logger, 
            IConfigurationManager<OpenIdConnectConfiguration> oidcConfigMgr, 
            AppConfiguration appConfig)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _oidcConfigMgr = oidcConfigMgr;
            _userIdJwtClaim = appConfig.Auth.UserIdJwtClaim;
        }

        public async Task<User?> GetUserFromJwtAsync(string jwtToken)
        {
            User? user = null;
            try
            {   
                var claimsPrincipal = await GetClaimsPrincipalFromJwtAsync(jwtToken);
                if (claimsPrincipal.Claims.FirstOrDefault(i => i.Type == _userIdJwtClaim) != null)
                {
                    #pragma warning disable CS8604, CS8629
                    Guid userId = Guid.Parse(claimsPrincipal.Claims.First(i => i.Type == _userIdJwtClaim).Value);
                    #pragma warning restore CS8604, CS8629
                    using (IServiceScope scope = _scopeFactory.CreateScope())
                    {
                        IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        user = await userService.GetUser(userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting user from jwt token");
            }
            return user;
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalFromJwtAsync(string jwtToken)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            OpenIdConnectConfiguration discoveryDocument = await _oidcConfigMgr.GetConfigurationAsync(new CancellationToken());
            List<SecurityKey> signingKeys = discoveryDocument.SigningKeys.ToList();
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = discoveryDocument.Issuer,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = discoveryDocument.SigningKeys.ToList()
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;
            ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out validatedToken);
            return claimsPrincipal;
        }

        public async Task<List<string>> GetEmailsFromJwtAsync(string jwtToken)
        {
            ClaimsPrincipal claimsPrincipal = await GetClaimsPrincipalFromJwtAsync(jwtToken);
            var emails = new List<string>();
            if (claimsPrincipal.Claims.Where(i => i.Type == "email").Any())
            {
                var email = claimsPrincipal.Claims.First(i => i.Type == "email").Value;
                if (email != null) emails.Add(email);
            } 
            if (claimsPrincipal.Claims.Where(i => i.Type == "emailAddresses" || i.Type == "emails").Any())
            {
                var newEmails = claimsPrincipal
                    .Claims
                    .Where(i => i.Type == "emailAddresses" || i.Type == "emails")
                    .Select(i => i.Value)
                    .ToList();
                emails.AddRange(newEmails);
            }
            if (emails.Count == 0)
            {
                throw new Exception("No email address found in jwt token");
            }
            return emails;
        }

        public async Task<string> GetJwtSubjectFromJwtAsync(string jwtToken)
        {
            ClaimsPrincipal claimsPrincipal = await GetClaimsPrincipalFromJwtAsync(jwtToken);
            string jwtSubject = claimsPrincipal.Claims.First(i => i.Type == "sub").Value;
            return jwtSubject;
        }
    }
}