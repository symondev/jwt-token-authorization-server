using Ezikey.Cloud.Services.Infrastructure.JwtTokenAuthorizationServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jwt.Token.Authorization.Server
{
    public static class JwtTokenAuthorizationServerAppBuilderExtensions
    {
        public static IApplicationBuilder UseJwtTokenAuthorizationServer(this IApplicationBuilder app, JwtTokenProviderOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<JwtTokenProviderMiddleware>(Options.Create(options));
        }
    }
}
