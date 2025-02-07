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

            using var transaction = await connection.BeginTransactionAsync(); // ⬅️ Inicia a transação

            try
            {
                // Atualiza a Agenda
                await connection.ExecuteAsync(
                    @"UPDATE Agenda 
            SET 
                Data = @Data,
                HoraInicio = @HoraInicio,
                HoraFim = @HoraFim,
                Disponivel = @Disponivel,
                PrecoConsulta = @PrecoConsulta
            WHERE Id = @Id",
                    agenda, transaction); // ⬅️ Passa a transação

                // Atualiza Consultas associadas à Agenda
                await connection.ExecuteAsync(
                    @"UPDATE Consultas 
            SET 
                Data = @Data,
                Hora = @HoraInicio,
                Status = CASE 
                    WHEN @Disponivel = FALSE THEN 'Cancelada'
                    ELSE Status
                END
            WHERE 
                AgendaId = @Id
            AND
                Status IN ('Agendada', 'Aprovada')
                ",
                    agenda, transaction); // ⬅️ Passa a transação

                await transaction.CommitAsync(); // ⬅️ Confirma as mudanças apenas se ambas as queries forem bem-sucedidas
            }
            catch
            {
                await transaction.RollbackAsync(); // ⬅️ Desfaz tudo se algo der errado
                throw; // ⬅️ Repropaga o erro
            }
        }

        public async Task DeleteScheduleAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync(); // ⬅️ Inicia a transação

            try
            {


                // Atualiza a Agenda
                await connection.ExecuteAsync(
                    @"UPDATE Agenda 
            SET 
                Disponivel = FALSE
            WHERE Id = @id",
                    new { id }, transaction); // ⬅️ Passa a transação

                // Atualiza Consultas associadas à Agenda
                await connection.ExecuteAsync(
                    @"UPDATE Consultas 
            SET 
                Status = 'Cancelada'
            WHERE 
                AgendaId = @id
            AND
                Status IN ('Agendada', 'Aprovada')
                ",
                    new { id }, transaction); // ⬅️ Passa a transação

                await transaction.CommitAsync(); // ⬅️ Confirma as mudanças apenas se ambas as queries forem bem-sucedidas
            }
            catch
            {
                await transaction.RollbackAsync(); // ⬅️ Desfaz tudo se algo der errado
                throw; // ⬅️ Repropaga o erro
            }
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

        public async Task<AgendaDadosModel> GetScheduleDadosByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @$"SELECT 
                          A.Id 
                        , C.Id ConsultaId
                        , P.Nome NomePaciente
                        , P.Email PacienteEmail
                        , P.Id PacienteId
                        , M.Nome NomeMedico
                        , M.Email MedicoEmail
                        , M.Id MedicoId
                        , A.Data
                        , A.HoraInicio
                        , A.HoraFim
                        , A.Disponivel
                        , A.PrecoConsulta
                    FROM Agenda A  
                INNER JOIN
                    Consultas C
                ON
                    A.ID = C.AgendaId
                INNER JOIN
                    Pacientes P
                ON
                    P.Id = C.PacienteId
                INNER JOIN
                    Medicos M
                ON 
                    M.Id = A.MedicoId
                  WHERE 
                        A.Id = @id 
                  AND 
                        C.Status IN('Agendada','Aprovada')

                    ";


            return await connection.QueryFirstOrDefaultAsync<AgendaDadosModel>(sql, new { id });
        }
    }

}

