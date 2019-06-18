using System;

namespace SecurityLogicLibraryContracts
{
   public  interface IResponsive : IDisposable
   {
       void InsertResponsive(string userId, int applicationId, int roleId, string userValidator);

       void AuthorizeResponsive(int responsiveId);
   }
}
