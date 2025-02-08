using Dapper;
using Npgsql;
using ScheduleSync.Domain.Entities;
using ScheduleSync.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ScheduleSync.Infrastructure.Repositories
{
    public class ConsultasRepository : IConsultasRepository
    {
        private readonly string _connectionString;

        public ConsultasRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<ConsultaModel>> GetConsultationsByDoctorIdAsync(int doctorId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.QueryAsync<ConsultaModel>(
                "SELECT * FROM Consultas WHERE MedicoId = @doctorId", new { doctorId });
        }

        public async Task<ConsultaModel> CreateConsultationAsync(ConsultaModel consulta)
        {
            var insertQuery = @"
                    INSERT INTO Consultas (PacienteId, MedicoId, Data, Hora, Status, JustificativaCancelamento, AgendaId)
                    VALUES (@PacienteId, @MedicoId, @Data, @Hora, @Status, @JustificativaCancelamento, @AgendaId)
                    RETURNING Id;
                        ";

            var updateQuery = @"
                            UPDATE Agenda SET DISPONIVEL = FALSE WHERE ID = @AgendaId;
                                ";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync(); // Inicia a transação

            try
            {
                int id = await connection.ExecuteScalarAsync<int>(insertQuery, consulta, transaction);
                await connection.ExecuteAsync(updateQuery, consulta, transaction);

                await transaction.CommitAsync(); // Confirma a transação

                return await GetConsultationByIdAsync(id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); // Desfaz a transação em caso de erro
                throw;
            }
        }


        public async Task<ConsultaModel> GetConsultationByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<ConsultaModel>(
                "SELECT * FROM Consultas WHERE Id = @id", new { id });
        }

        public async Task UpdateConsultationAsync(ConsultaModel consulta)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                @"UPDATE Consultas SET 
                    PacienteId = @PacienteId,
                    MedicoId = @MedicoId,
                    Data = @Data,
                    Hora = @Hora,
                    Status = @Status,
                    JustificativaCancelamento = @JustificativaCancelamento
                WHERE Id = @Id", consulta);
        }

        public async Task DeleteConsultationAsync(int id, string justificativa)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                @"UPDATE Consultas SET 
                    Status = 'Cancelada',
                    JustificativaCancelamento = @justificativa
                WHERE Id = @id", new { id, justificativa });
        }

        public async Task ApproveConsultationAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                @"UPDATE Consultas SET 
                    Status = 'Aprovada'
                WHERE Id = @id", new { id });
        }
    }
}
