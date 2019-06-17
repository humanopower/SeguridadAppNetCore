using System;
using System.Collections.Generic;
using EntityLibrary;

namespace DataAccessContracts
{
    public interface ILogDA : IDisposable
    {
        /// <summary>
        /// Metodo que agrega un registro de bitacora en el esquema de seguridad.
        /// </summary>
        /// <param name="log">Objeto tipo Log</param>
        void AddLogEvent(Log log);


        /// <summary>
        /// MEtodo que obtiene lista de eventos.
        /// </summary>
        /// <returns>Lista tipo Log</returns>
        List<Log> GetLogList();
    }
}
