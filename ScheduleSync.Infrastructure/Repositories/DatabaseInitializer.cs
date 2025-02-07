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
            try
            {
                var createAgendaTableQuery = @"
                CREATE TABLE IF NOT EXISTS Agenda (
                    Id SERIAL PRIMARY KEY,
                    MedicoId INT NOT NULL,
                    Data Date NOT NULL,
                    HoraInicio TIME,
                    HoraFim TIME,
                    Disponivel BOOLEAN,
                    PrecoConsulta DECIMAL(10,2)
                );
            ";

                var createConsultasTableQuery = @"
                CREATE TABLE IF NOT EXISTS Consultas (
                    Id SERIAL PRIMARY KEY,
                    PacienteId INT NOT NULL,
                    MedicoId INT NOT NULL,
                    Data Date NOT NULL,
                    Hora TIME,
                    Status VARCHAR(20),
                    JustificativaCancelamento TEXT,
                    AgendaId INT NOT NULL
                );
            ";

                _dbConnection.Execute(createAgendaTableQuery);
                _dbConnection.Execute(createConsultasTableQuery);
            }
            catch (Exception e)
            {

                throw;
            }
            
        }
    }
}

