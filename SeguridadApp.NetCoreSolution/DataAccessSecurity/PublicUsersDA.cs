using System;
using DataAccessContracts;

namespace DataAccessSecurity
{
    public class PublicUsersDA : IPublicUsersDA

{
    public string GetLastEncryptedPassword(string login)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
       GC.SuppressFinalize(this);
    }
}
}
