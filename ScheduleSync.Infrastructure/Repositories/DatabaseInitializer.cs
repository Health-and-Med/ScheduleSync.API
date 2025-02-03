using ScheduleSync.Domain.Interfaces;
using Dapper;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ScheduleSync.Infrastructure.Repositories
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDbConnection _dbConnection;

        public DatabaseInitializer(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void Initialize()
        {
            var createScheduleTableQuery = @"
                CREATE TABLE IF NOT EXISTS Schedule (
                    Id SERIAL PRIMARY KEY,
                    DoctorId INT NOT NULL,
                    PatientId INT NOT NULL,
                    ScheduledDate TIMESTAMP 
                );
            ";

            _dbConnection.Execute(createScheduleTableQuery);
        }
    }
}

