using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Ezikey.Cloud.Services.Infrastructure.JwtTokenAuthorizationServer
{
    public class JwtTokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtTokenProviderOptions _options;
        private readonly IJwtTokenProviderBehavior _behavior;
        private readonly ILogger<JwtTokenProviderMiddleware> _logger;

        public JwtTokenProviderMiddleware(
            RequestDelegate next,
            IOptions<JwtTokenProviderOptions> options,
            IJwtTokenProviderBehavior behavior,
            ILogger<JwtTokenProviderMiddleware> logger = null)
        {
            if (behavior == null)
            {
                throw new ArgumentNullException(nameof(behavior));
            }

            _next = next;
            _options = options.Value;
            _behavior = behavior;
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
            if (!context.Request.Method.Equals("POST")
               || !context.Request.HasFormContentType)
            {
                LogError($"Bad request. Request method is {context.Request.Method}. Request Content Type is {context.Request.ContentType}");

                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }

            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            LogDebug("Attempting to get identity.");

            var identity = await _behavior.GetIdentity(context);
            if (identity == null)
            {
                LogError("Invalid username or password.");

                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid username or password.");
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

            LogDebug("Attempting to get custom claims.");

            var customClaims = _behavior.CustomClaims(context, identity);
            if (customClaims != null)
            {
                claims.AddRange(customClaims);
            }

            // Create the JWT and write it to a string
            LogDebug("Attempting to generate jwt token.");

            var jwtHeader = new JwtHeader(_options.SigningCredentials);
            var jwtPayload = new JwtPayload(
                issuer:_options.Issuer,
                audience:_options.Audience,
                claims: claims,
                notBefore:now,
                expires: now.Add(_options.Expiration),
                issuedAt:now);

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
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
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
