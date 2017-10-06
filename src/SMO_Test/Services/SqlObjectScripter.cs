using Microsoft.SqlServer.Management.Smo;
using System;
using System.Text;

namespace SMO_Test
{
    public class SqlObjectScripter : IDisposable
    {
        public const string BATCH_SEPERATOR = "## BATCH SEPERATOR ##";
        private readonly Server _server;
        private readonly Database _db;

        public SqlObjectScripter(string server, string database, string username, string password)
        {
            _server = new Server();
            _server.ConnectionContext.LoginSecure = false;
            _server.ConnectionContext.Login = username;
            _server.ConnectionContext.Password = password;
            _server.ConnectionContext.ServerInstance = server;

            _db = _server.Databases[database];
            if (_db == null) throw new Exception($"Could not connect to database {database} on {server}");
        }

        public string GenerateScript(string schema, string objectName, string objectType)
        {
            switch (objectType)
            {
                case "TABLE":
                    return GenerateCreateTableScript(schema, objectName);
                case "VIEW":
                    return GenerateViewScript(schema, objectName);
                case "PROCEDURE":
                    return GenerateStoredProcedureScript(schema, objectName);
                case "FUNCTION":
                    return GenerateFunctionScript(schema, objectName);
                default:
                    return null;
            }
        }

        private string GenerateCreateTableScript(string schema, string table)
        {
            Table tbl = _db.Tables[table, schema];
            if (tbl == null) return null;
            return Script(tbl);
        }

        private string GenerateViewScript(string schema, string view)
        {
            View vw = _db.Views[view, schema];
            if (vw == null) return null;
            return Script(vw);
        }

        private string GenerateFunctionScript(string schema, string function)
        {
            IScriptable func = _db.UserDefinedFunctions[function, schema];
            if (func == null) return null;
            return Script(func);
        }

        private string GenerateStoredProcedureScript(string schema, string storedProcedure)
        {
            StoredProcedure sp = _db.StoredProcedures[storedProcedure, schema];
            if (sp == null) return null;
            return Script(sp);
        }

        private string Script(IScriptable obj, ScriptingOptions options = null)
        {
            if (obj == null) return null;
            if (options == null)
            {
                options = new ScriptingOptions();
                options.ClusteredIndexes = true;
                options.Default = true;
                options.DriAll = true;
                options.Indexes = true;
                options.IncludeHeaders = true;
            }
            StringBuilder sb = new StringBuilder();
            var scripts = obj.Script(options);
            foreach (var script in scripts)
            {
                sb.AppendLine(script);
                sb.AppendLine(BATCH_SEPERATOR);
            }
            return sb.ToString();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _server.ConnectionContext.Disconnect();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
