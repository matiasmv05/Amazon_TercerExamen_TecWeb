using Amazon.Core.Enum;
using Amazon.Core.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Repositories
{
        public class DbConnectionFactory : IDbConnectionFactory
        {
            private readonly string _sqlConn;

            public DbConnectionFactory(IConfiguration config)
            {
                _sqlConn = config.GetConnectionString("ConnectionSqlServer")
                    ?? throw new InvalidOperationException("Connection string 'ConnectionSqlServer' not found.");

                Provider = DatabaseProvider.SqlServer;
            }

            public DatabaseProvider Provider { get; }

            public IDbConnection CreateConnection()
            {
                var connection = new SqlConnection(_sqlConn);
                return connection;
            }
        }
    

}
