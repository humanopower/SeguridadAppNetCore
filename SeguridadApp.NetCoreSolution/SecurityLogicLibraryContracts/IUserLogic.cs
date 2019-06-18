using System;
using System.Collections.Generic;
using EntityLibrary;

namespace SecurityLogicLibraryContracts
{
    public interface IUserLogic : IDisposable
    {
        #region Metodos Publicos

        Response AddNewUser(EntityLibrary.User user, EntityLibrary.User registerUser);

        Response DelUser(User user, User registerUser);

        Response UpdUser(User user, User registerUser);

        User FindUser(string userId);

        /// <summary>
        /// Metodo que regresa una lista de registros que coinciden con la búsqueda
        /// </summary>
        /// <param name="strValue">Valor a buscar</param>
        /// <returns>Regresa un objeto lista de los usuarios</returns>
        List<User> SearchUser(string strValue, User registerUser);


        List<User> SearchUserScope(string strValue, User registerUser);

        List<User> GetUserId(string strValue);

        string GetNameUserAccount(string numeroEmpleado);


        #endregion
    }
}
