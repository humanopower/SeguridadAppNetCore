using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessContracts
{
	public interface IUserDA : IDisposable
	{
		void AddUser(User user, User registerUser);

		void DelUser(User user, User registerUser);

		void UpdUser(User user, User registerUser);

		User FindUser(string userId);

		/// <summary>
		/// Metodo que busca usuarios por distintos campos
		/// </summary>
		/// <param name="strValue">Valor a buscar</param>
		/// <returns>Regresa objeto de tipo DataSet</returns>
		DataSet FindUsers(string strValue, User registerUser);

		List<User> GetUser(string strValue);

		DataSet FindUsersScope(string strValue, User registerUser);

		string GetNameUserAccount(string numberEmployeeNumber);
	}
}