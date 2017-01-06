using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ezikey.Cloud.Services.Infrastructure.JwtTokenAuthorizationServer
{
    public interface IJwtTokenProviderBehavior
    {
        Task<ClaimsIdentity> GetIdentity(HttpContext context);

        Claim[] CustomClaims(HttpContext context, ClaimsIdentity identity);
    }
}
