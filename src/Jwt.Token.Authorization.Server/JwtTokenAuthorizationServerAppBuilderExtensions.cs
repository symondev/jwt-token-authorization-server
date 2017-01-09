using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace Jwt.Token.Authorization.Server
{
    public static class JwtTokenAuthorizationServerAppBuilderExtensions
    {
        public static IApplicationBuilder UseJwtTokenAuthorizationServer(this IApplicationBuilder app, JwtTokenAuthorizationServerOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<JwtTokenAuthorizationServerMiddleware>(Options.Create(options));
        }
    }
}
