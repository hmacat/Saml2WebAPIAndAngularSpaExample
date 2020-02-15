using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        [HttpGet]
        public ActionResult Get()
        {
            // Retrieve jwt from session JWT.
            var jwt = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(jwt))
            {
                return Unauthorized();
            }

            return Ok(jwt);
        }

        [Authorize]
        [HttpGet("TestAuthorization")]
        public ActionResult TestAuthorization()
        {
            return this.Ok("Congratulations, you are authorized.");
        }
    }
}