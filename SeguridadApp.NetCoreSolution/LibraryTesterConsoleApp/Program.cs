using Microsoft.Extensions.Configuration;
using System;

namespace LibraryTesterConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			var dbConnectionClass = new DataAccessSecurity.ApplicationDA(config);
			var applicationList = dbConnectionClass.GetApplicationList();
			foreach (var item in applicationList)
			{
				Console.WriteLine(string.Format("Se encontró la aplicacion {0} de un total de {1}", item.ApplicationName, applicationList.Count));
			}
			Console.ReadLine();
		}
	}
}
