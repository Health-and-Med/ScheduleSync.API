using ScheduleSync.Domain.Entities;
using ScheduleSync.Domain.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleSync.Infrastructure.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly IDbConnection _dbConnection;

        public ScheduleRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Appointment> GetSheduleDoctor(int doctorId, DateTime scheduledDate)
        {
            string sql = "SELECT * FROM Schedule WHERE doctorId = @doctorId AND scheduledDate = @scheduledDate";
            return await _dbConnection.QueryFirstOrDefaultAsync<Appointment>(sql, new { doctorId, scheduledDate });
        }

        public async Task AddScheduleAsync(ScheduleAppointmentRequest schedule)
        {
            string sql = "INSERT INTO Schedule (doctorid, patientid, scheduleddate) VALUES (@DoctorId, @PatientId, @ScheduledDate)";
            await _dbConnection.ExecuteAsync(sql, schedule);
        }
    }
}

