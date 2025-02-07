using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ScheduleSync.Domain.Entities;

namespace ScheduleSync.Infrastructure.Repositories
{

    public class AgendaRepository : IAgendaRepository
    {
        private readonly string _connectionString;

        public AgendaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<AgendaModel>> GetScheduleByDoctorIdAsync(int doctorId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.QueryAsync<AgendaModel>(
                "SELECT * FROM Agenda WHERE MedicoId = @doctorId", new { doctorId });
        }

        public async Task<AgendaModel> CreateScheduleAsync(AgendaModel agenda)
        {
            var query = @"
                INSERT INTO Agenda (MedicoId, Data, HoraInicio, HoraFim, Disponivel, PrecoConsulta)
                VALUES (@MedicoId, @Data, @HoraInicio, @HoraFim, @Disponivel, @PrecoConsulta)
                RETURNING Id;";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            int id = await connection.ExecuteScalarAsync<int>(query, agenda);
            return await GetScheduleByIdAsync(id);
        }

        public async Task<AgendaModel> GetScheduleByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<AgendaModel>(
                "SELECT * FROM Agenda WHERE Id = @id", new { id });
        }

        public async Task UpdateScheduleAsync(AgendaModel agenda)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                @"UPDATE Agenda SET 
                    Data = @Data,
                    HoraInicio = @HoraInicio,
                    HoraFim = @HoraFim,
                    Disponivel = @Disponivel,
                    PrecoConsulta = @PrecoConsulta
                WHERE Id = @Id", agenda);
        }

        public async Task DeleteScheduleAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync("DELETE FROM Agenda WHERE Id = @id", new { id });
        }

        public async Task<IEnumerable<AgendaModel>> CreateMultipleSchedulesAsync(List<AgendaModel> agendas)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            var query = @"
        INSERT INTO Agenda (MedicoId, Data, HoraInicio, HoraFim, Disponivel, PrecoConsulta)
        VALUES (@MedicoId, @Data, @HoraInicio, @HoraFim, @Disponivel, @PrecoConsulta)
        RETURNING *;
    ";

            try
            {
                var insertedSchedules = new List<AgendaModel>();

                foreach (var agenda in agendas)
                {
                    var result = await connection.QuerySingleAsync<AgendaModel>(query, agenda, transaction);
                    insertedSchedules.Add(result);
                }

                await transaction.CommitAsync();
                return insertedSchedules;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

}

