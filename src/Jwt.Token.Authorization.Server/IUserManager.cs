using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jwt.Token.Authorization.Server
{
    public interface IUserManager
    {
        Task<ClaimsIdentity> GetIdentity(HttpContext context);
    }
}
