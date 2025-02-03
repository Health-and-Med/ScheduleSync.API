using ScheduleSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleSync.Domain.Interfaces
{
    public interface IScheduleRepository
    {
        Task AddScheduleAsync(ScheduleAppointmentRequest schedule);
        Task<Appointment> GetSheduleDoctor(int doctorId, DateTime scheduledDate);
    }
}

