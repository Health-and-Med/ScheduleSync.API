using ScheduleSync.Domain.Entities;
using ScheduleSync.Domain.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleSync.Application.Services
{
    public class SheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly byte[] _key;

        public SheduleService(IScheduleRepository userRepository)
        {
            _scheduleRepository = userRepository;
            // Usar uma chave fixa para HMACSHA512 (isso é para simplificação, para produção você deve usar um salt único para cada senha)
            _key = Encoding.UTF8.GetBytes("a-secure-key-of-your-choice");
        }

        public async Task AddScheduleAsync(ScheduleAppointmentRequest schedule)
        {
            await _scheduleRepository.AddScheduleAsync(schedule);
        }

        public async Task<Appointment> GetSheduleDoctor(int doctorId, DateTime scheduledDate)
        {
            return await _scheduleRepository.GetSheduleDoctor(doctorId, scheduledDate);
        }
    }
}

