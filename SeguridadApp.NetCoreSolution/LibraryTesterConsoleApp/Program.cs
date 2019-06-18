using Microsoft.Extensions.Configuration;
using System;

namespace LibraryTesterConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Testing DbClass");
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			var dbConnectionClass = new DataAccessSecurity.ApplicationDA(config);
			var applicationList = dbConnectionClass.GetApplicationList();
			foreach (var item in applicationList)
			{
				Console.WriteLine(string.Format("Libreria DB : Se encontró la aplicacion {0} de un total de {1}", item.ApplicationName, applicationList.Count));
			}
			Console.ReadLine();

			Console.WriteLine("TestingLibraryClass");
		
			var dbLogic = new SecurityLogicLibrary.ApplicationLogic(config);
			var applicationListLogic = dbLogic.GetApplicationList();
			foreach (var item in applicationListLogic)
			{
				Console.WriteLine(string.Format("Libreria Logic : Se encontró la aplicacion {0} de un total de {1}", item.ApplicationName, applicationList.Count));
			}
			Console.ReadLine();
		}
	}
}
