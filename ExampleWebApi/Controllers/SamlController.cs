using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sustainsys.Saml2.AspNetCore2;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ExampleWebApi.Constants;

namespace ExampleWebApi.Controllers
{
    [Route("api/Saml")]
    public class SamlController : Controller
    {
        private readonly IConfiguration configuration;

        public SamlController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("InitiateSingleSignOn")]
        public IActionResult InitiateSingleSignOn(string returnUrl)
        {
            return new ChallengeResult(
                Saml2Defaults.Scheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(LoginCallback), new { returnUrl })
                });
        }

        [AllowAnonymous]
        [HttpGet("Callback")]
        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(ApplicationSamlConstants.External);

            if (!authenticateResult.Succeeded)
            {
                return Unauthorized();
            }

            var token = this.CreateJwtSecurityToken(authenticateResult);
            HttpContext.Session.SetString("JWT", new JwtSecurityTokenHandler().WriteToken(token));

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);

            }

            return this.Ok();
        }

        private JwtSecurityToken CreateJwtSecurityToken(AuthenticateResult authenticateResult)
        {
            var claimsIdentity = new ClaimsIdentity(ApplicationSamlConstants.Application);
            claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier));

            var username = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["JWT:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                this.configuration["JWT:Issuer"],
                this.configuration["JWT:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);
        }
    }
}