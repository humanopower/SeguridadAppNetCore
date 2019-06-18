using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityLibrary;
using Microsoft.AspNetCore.Mvc;

namespace SeguridadAppAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		// GET api/values
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<Response> Get(int id)
		{
			var resp = new Response();
			resp.Message = "HEllo";
			resp.Result = true;

			return resp;
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
