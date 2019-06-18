using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SeguridadAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityServiceController : ControllerBase
    {
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
	   public IActionResult Authenticate([FromBody] Response value)
		{
			//[FromBody] string value
			var resp = new Response();
			resp.Message = "UsuarioAutenticado";
			resp.Result = true;
			return Ok(resp);
		}
	}
}