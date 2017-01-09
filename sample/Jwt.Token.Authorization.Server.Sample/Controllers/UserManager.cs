using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Jwt.Token.Authorization.Server.Sample.Controllers
{
    public class UserManager : IUserManager
    {
        public async Task<ClaimsIdentity> GetIdentity(HttpContext context)
        {
            if (context.Request.Form["username"] == "test" && context.Request.Form["password"] == "test")
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Name, "test"));
                identity.AddClaim(new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(new {Custom1 = 1, Custom2 =2})));

                return await Task.FromResult(identity);
            }
            else
            {
                return null;
            }
        }
    }
}
