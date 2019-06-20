using EntityLibrary;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SecurityServicesContracts;
using System;
using System.Collections.Generic;

namespace SeguridadAppAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SecurityServiceController : ControllerBase
	{

		private readonly ISecurityServiceContract _securityService;
		public SecurityServiceController(ISecurityServiceContract securityService)
		{
			_securityService = securityService;
		}


		// POST api/values
		[HttpGet]
		public IActionResult Get()
		{
			var resp = new Response();
			resp.Message = "Welcome to the SecurityService. This is test basic Get method. Refer to manual for allowed operations.";
			resp.Result = true;
			resp.Result = true;

			return Ok(resp);
		}

		/// <summary>
		/// Returns a true or false response against Active Directory and Application Security Service
		/// </summary>
		// POST api/Authenticate
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
				
				resp = _securityService.Authenticate(domain, userId, password, applicationName, out userAuthenticated);
				
				var returnObject = new { Response = resp, User = userAuthenticated };
				return Ok(returnObject);
			}
			catch (Exception ex)
			{
				return BadRequest("La solicitud no fué enviada correctamente");
			}
		}

		/// <summary>
		/// Returns a true or false response against Active Directory only.
		/// </summary>
		// POST api/AuthenticateADOnly
		[HttpPost("AuthenticateADOnly")]
		public IActionResult AuthenticateADOnly([FromBody] JObject data)
		{
			var resp = new Response();
			var userAuthenticated = new User();
			try
			{
				var domain = data["domain"].ToString();
				var userId = data["userId"].ToString();
				var password = data["password"].ToString();
				resp = _securityService.AuthenticateADOnly(domain, userId, password, out userAuthenticated);
				var returnObject = new { Response = resp, User = userAuthenticated };
				return Ok(returnObject);
			}
			catch (Exception ex)
			{
				return BadRequest("La solicitud no fué enviada correctamente");
			}
		}

		/// <summary>
		/// Returns a true or false response against Security schema
		/// </summary>
		// POST api/Authorize
		[HttpPost("Authorize")]
		public IActionResult Authorize([FromBody] JObject data)
		{
			var resp = new Response();
			var userAuthenticated = new User();
			try
			{
				userAuthenticated = JsonConvert.DeserializeObject<User>(data["user"].ToString());
				var applicationName = data["applicationName"].ToString();
				var applicationPassword = data["applicationPassword"].ToString();
				var operation = data["operation"].ToString();
				resp = _securityService.Authorize(userAuthenticated, applicationName, applicationPassword, operation);
				
				var returnObject = new { Response = resp };
				return Ok(returnObject);
			}
			catch (Exception ex)
			{
				return BadRequest("La solicitud no fué enviada correctamente");
			}
		}

		/// <summary>
		/// Get user information and roles assigned to an user.
		/// </summary>
		// POST api/GetUserInformationAndRoles
		[HttpPost("GetUserInformationAndRoles")]
		public IActionResult GetUserInformationAndRoles([FromBody] JObject data)
		{
			var resp = new Response();
			var user = new User();
			var roleList = new List<Role>();
			try
			{
				var applicationName = data["applicationName"].ToString();
				var applicationPassword = data["applicationPassword"].ToString();
				var userId = data["userId"].ToString();
				
				resp = _securityService.GetUserInformationAndRoles(applicationName, applicationPassword, userId, out user, out roleList);
				
				var returnObject = new { Response = resp, User = user, roleUserList = roleList };
				return Ok(returnObject);
			}
			catch (Exception ex)
			{
				return BadRequest("La solicitud no fué enviada correctamente");
			}
		}

		/// <summary>
		/// Get user information and Operations assigned to an user.
		/// </summary>
		// POST api/GetUserInformationAndOperations
		[HttpPost("GetUserInformationAndOperations")]
		public IActionResult GetUserInformationAndOperations([FromBody] JObject data)
		{
			var resp = new Response();
			var operationList = new List<Operation>();
			var userAuthenticated = new User();
			try
			{
				userAuthenticated = JsonConvert.DeserializeObject<User>(data["user"].ToString());
				var applicationName = data["applicationName"].ToString();
				var applicationPassword = data["applicationPassword"].ToString();
				var userId = data["userId"].ToString();
				resp = _securityService.GetUserInformationAndOperations(userAuthenticated, applicationName, applicationPassword, out operationList);
				
				var returnObject = new { Response = resp, operationUserList = operationList };
				return Ok(returnObject);
			}
			catch (Exception ex)
			{
				return BadRequest("La solicitud no fué enviada correctamente");
			}
		}

		/// <summary>
		/// Get user list by role
		/// </summary>
		// POST api/GetUserListByRole
		[HttpPost("GetUserListByRole")]
		public IActionResult GetUserListByRole([FromBody] JObject data)
		{
			var resp = new Response();
			var operationList = new List<Operation>();
			var userListFindedByRole = new List<User>();
			try
			{
				var applicationName = data["applicationName"].ToString();
				var applicationPassword = data["applicationPassword"].ToString();
				var roleName = data["roleName"].ToString();
					resp = _securityService.GetUserListByRole(applicationName, applicationPassword, roleName, out userListFindedByRole);
				var returnObject = new { Response = resp, userList = userListFindedByRole };
				return Ok(returnObject);
			}
			catch (Exception ex)
			{
				return BadRequest("La solicitud no fué enviada correctamente");
			}
		}

		/// <summary>
		/// Get user list by Application
		/// </summary>
		// POST api/GetUserListByRole
		[HttpPost("GetUserListByApplication")]
		public IActionResult GetUserListByApplication([FromBody] JObject data)
		{
			var resp = new Response();
			var operationList = new List<Operation>();
			var userListFindedByApplication = new List<User>();
			try
			{
				var applicationName = data["applicationName"].ToString();
				var applicationPassword = data["applicationPassword"].ToString();
				
					resp = _securityService.GetUserListByApplication(applicationName, applicationPassword, out userListFindedByApplication);
				
				var returnObject = new { Response = resp, userList = userListFindedByApplication };
				return Ok(returnObject);
			}
			catch (Exception ex)
			{
				return BadRequest("La solicitud no fué enviada correctamente");
			}
		}
	}
}