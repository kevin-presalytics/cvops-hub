using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Serilog;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Threading;
using lib.models.configuration;
using lib.models;

namespace lib.extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddCVOpsAuth(this IServiceCollection services, AppConfiguration appConfig, ILogger logger)
        {
            
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var oidcConfigMgr = new ConfigurationManager<OpenIdConnectConfiguration>(
                appConfig.Auth.Oidc.WellKnownEndpoint,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever()
            );
        
            OpenIdConnectConfiguration discoveryDocument = oidcConfigMgr.GetConfigurationAsync(new CancellationToken()).GetAwaiter().GetResult();
            List<SecurityKey> signingKeys = discoveryDocument.SigningKeys.ToList();
    
            services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(oidcConfigMgr);
            
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; 
                }).AddJwtBearer(options => 
                {
                    // options.Authority = discoveryDocument.GetAuthority();  // Not needed?
                    options.MetadataAddress = appConfig.Auth.Oidc.WellKnownEndpoint;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters(){
                        ValidateIssuer = true,
                        ValidIssuer = discoveryDocument.Issuer, 
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = discoveryDocument.SigningKeys.ToList()
                    };
                    options.IncludeErrorDetails = true;
                    options.Events = new JwtBearerEvents() {
                        OnAuthenticationFailed = c => {
                            logger.Debug("Unauthorized Request. {0}", c.Exception.Message);
                            return Task.CompletedTask;
                         },
                        OnChallenge = c => {
                            logger.Debug(string.Format("Token Challenged"));
                            c.HandleResponse();
                            return Task.CompletedTask;
                        },
                        OnForbidden = c => {
                            logger.Information("Token forbidden");
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = c => {
                            logger.Debug("Message Received");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = c => {
                            logger.Debug("Token Validated");
                            return Task.CompletedTask;
                        }
                    };
                });
        
            services.AddAuthorization();

            return services;
        }
    }
}