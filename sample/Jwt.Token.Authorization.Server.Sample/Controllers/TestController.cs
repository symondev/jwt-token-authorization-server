using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Jwt.Token.Authorization.Server.Sample.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TestController : Controller
    {
        public string Get()
        {
            var user = this.Request.HttpContext.User;
            return "Test is ok";
        }
    }
}
