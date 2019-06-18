using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibraryTester
{
	[TestClass]
	class DataBaseBasicTester
	{
		[TestMethod]
		public void DatabaseConnection()
		{
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			var dbConnectionClass = new DataAccessSecurity.ApplicationDA(config);
			var applicationList = dbConnectionClass.GetApplicationList();
			Assert.IsTrue(applicationList.Count != 0);
		}
	}
}
