using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jwt.Token.Authorization.Server
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseJwtTokenAuthorizationServer(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<JwtTokenProviderMiddleware>();
        }
    }
}
