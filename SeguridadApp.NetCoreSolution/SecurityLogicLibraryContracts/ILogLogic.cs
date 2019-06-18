using System;
using System.Collections.Generic;
using EntityLibrary;

namespace SecurityLogicLibraryContracts
{
    public interface ILogLogic : IDisposable
    {

        Response InsertLogEvent(Log log);

        List<Log> GetLogList();
    }
}
