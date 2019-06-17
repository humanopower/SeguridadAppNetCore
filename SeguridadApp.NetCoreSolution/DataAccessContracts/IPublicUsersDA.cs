using System;

namespace DataAccessContracts
{
    public interface IPublicUsersDA : IDisposable
    {
        string GetLastEncryptedPassword(string login);
    }
}
