using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SecurityLogicLibrary;

namespace SeguridadAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityServiceController : ControllerBase
    {
		private readonly IConfiguration _config;
		public SecurityServiceController(IConfiguration configuration)
		{
			_config = configuration;
		}

		// POST api/values
		[HttpPost]
		public ActionResult<Response> Post([FromBody] string value)
		{
			var resp = new Response();
			resp.Message = "HEllo";
			resp.Result =true;
			return resp;
		}

		// POST api/values
		[HttpGet]
		public IActionResult Get()
		{
			var resp = new Response();
			resp.Message = "HEllo";
			resp.Result = true;
			
			return Ok(resp);
		}

		
		// POST api/values
		[HttpPost("Authenticate")]
	   public IActionResult Authenticate([FromBody] JObject data)
		{

			var resp = new Response();
			var userAuthenticated = new User();
			try
			{
				var domain = data["domain"].ToString();
				var userId = data["userId"].ToString();
				var password = data["password"].ToString();
				var applicationName = data["applicationName"].ToString();
				var securityServiceOperation = new SecurityServiceOperations(_config);				
				resp = securityServiceOperation.Authenticate(domain, userId, password, applicationName, out userAuthenticated);
				var returnObject = new { Response = resp, User = userAuthenticated };
				return Ok(returnObject);
			}
			catch (Exception ex)
			{
				return BadRequest("Algo salió mal");
			}
			
			
		}
	}
}