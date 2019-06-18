using System;
using DataAccessSecurity;
using Microsoft.Extensions.Configuration;
using SecurityLogicLibraryContracts;

namespace SecurityLogicLibrary
{
    public class Responsive : IResponsive
    {
		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public Responsive(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion

		public void InsertResponsive(string userId, int applicationId, int roleId, string userValidator)
        {
            var responsiveDa = new ResponsiveDA(_configuration);
            responsiveDa.AddUserResponsive(userId, applicationId, roleId, userValidator);
            responsiveDa.Dispose();
        }

        public void AuthorizeResponsive(int responsiveId)
        {
            var responsiveDa = new ResponsiveDA(_configuration);
            responsiveDa.AuthorizeUserResponsive(responsiveId);
            responsiveDa.Dispose();
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
