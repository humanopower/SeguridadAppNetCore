using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibraryTester
{
	[TestClass]
	class LogicBasicTester
	{
		[TestMethod]
		public void GetApplicationListConnection()
		{
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			var logicApplicationClass = new SecurityLogicLibrary.ApplicationLogic(config);
			var applicationList = logicApplicationClass.GetApplicationList();
			Assert.IsTrue(applicationList.Count != 0);
		}
	}
}
