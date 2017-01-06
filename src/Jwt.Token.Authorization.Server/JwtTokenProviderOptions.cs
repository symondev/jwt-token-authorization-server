using System;
using Microsoft.IdentityModel.Tokens;

namespace Ezikey.Cloud.Services.Infrastructure.JwtTokenAuthorizationServer
{
    public class JwtTokenProviderOptions
    {
        public string Path { get; set; } = "/token";

        public string Subject { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);

        public SigningCredentials SigningCredentials { get; set; }


    }
}
