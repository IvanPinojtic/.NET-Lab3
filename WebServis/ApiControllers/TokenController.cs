using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebServis.DAL.Entities;
using WebServis.Models;

namespace WebServis.ApiControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("api/Token")]
    //[Route("api/v{version:apiVersion}/Token")]
    [AllowAnonymous]
    public class TokenController : Controller
    {
        private readonly AdventureWorks2014Context _context;

        public IConfiguration Configuration { get; set; }

        public TokenController(IConfiguration configuration, AdventureWorks2014Context context)
        {
            Configuration = configuration;
            _context = context;
        }

        [HttpPost("RequestToken")]
        public IActionResult RequestToken([FromBody] TokenRequestModel tokenRequest)
        {
            if (_context.Person.Any(c => c.FirstName == tokenRequest.FirstName))
            {
                JwtSecurityToken token = JwsTokenCreator.CreateToken(tokenRequest.FirstName,
            Configuration["Auth:JwtSecurityKey"],
            Configuration["Auth:ValidIssuer"],
            Configuration["Auth:ValidAudience"]);
                string tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(tokenStr);
            }
            return Unauthorized();
        }

        [HttpGet("RequestTokenVersion")]
        [HttpPost("RequestTokenVersion")]
        [MapToApiVersion("1.0"), MapToApiVersion("1.1")]
        public string GetApiVersion() => HttpContext.GetRequestedApiVersion().ToString();

    }

    [Produces("application/json")]
    [ApiVersion("1.1")]
    [Route("api/v{version:apiVersion}/Token")]
    [AllowAnonymous]
    public class TokenV1_1Controller : Controller
    {
        private readonly AdventureWorks2014Context _context;

        public IConfiguration Configuration { get; }

        public TokenV1_1Controller(AdventureWorks2014Context context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        [HttpPost("RequestToken")]
        public async Task<IActionResult> RequestToken([FromBody] TokenRequestModel tokenRequest)
        {
            var person = await _context.Person.FirstOrDefaultAsync(c => c.FirstName == tokenRequest.FirstName);
            if (person!=null)
            {
                JwtSecurityToken token = JwsTokenCreator.CreateToken(tokenRequest.FirstName,
            Configuration["Auth:JwtSecurityKey"],
            Configuration["Auth:ValidIssuer"],
            Configuration["Auth:ValidAudience"]);
                string tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(tokenStr);
            }
            return Unauthorized();
        }
    }
}