using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Symon.AspNetCore.Mvc.ExceptionHandler.Abstractions;

namespace Jwt.Token.Authorization.Server
{
    public class JwtTokenAuthorizationServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtTokenAuthorizationServerOptions _options;
        private readonly ILogger<JwtTokenAuthorizationServerMiddleware> _logger;

        public JwtTokenAuthorizationServerMiddleware(
            RequestDelegate next,
            IOptions<JwtTokenAuthorizationServerOptions> options,
            ILogger<JwtTokenAuthorizationServerMiddleware> logger = null)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                return _next(context);
            }

            LogDebug($"Accessed path {_options.Path}");

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST") || !context.Request.HasFormContentType)
            {
                var errMessage = $"Bad request. Request method is {context.Request.Method}. Request Content Type is {context.Request.ContentType}";
                LogError(errMessage);

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(
                    new ApiErrorResult
                    {
                        Type = ErrorType.Unexpected,
                        Message = errMessage
                    }
                    , Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver =
                            new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                    }));
            }

            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            LogDebug("Attempting to get identity.");

            var userManager = context.RequestServices.GetService<IUserManager>();
            if (userManager == null)
            {
                LogDebug("Did not implement IUserManager.");
                throw new Exception("Please implement IUserManager.");
            }

            var identity = await userManager.GetIdentity(context);
            if (identity == null)
            {
                LogError("Invalid username or password.");

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(
                    new ApiErrorResult
                    {
                        Type = ErrorType.Unexpected,
                        Message = "Invalid username or password."
                    }, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver =
                            new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                    }));
                return;
            }

            var now = DateTime.UtcNow;

            // Create clamins
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (!string.IsNullOrEmpty(_options.Subject))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, _options.Subject));
            }
            claims.AddRange(identity.Claims);

            // Create the JWT and write it to a string
            LogDebug("Attempting to generate jwt token.");

            var jwtHeader = new JwtHeader(_options.SigningCredentials);
            var jwtPayload = new JwtPayload(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                issuedAt: now);

            var jwt = new JwtSecurityToken(jwtHeader, jwtPayload);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            LogDebug($"Jwt token generated successful.");

            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int)_options.Expiration.TotalSeconds
            };

            // Serialize and return the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver =
                        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                }));
        }

        private void LogDebug(string message)
        {
            _logger?.LogDebug(message);
        }

        private void LogError(string message)
        {
            _logger?.LogError(message);
        }
    }
}
