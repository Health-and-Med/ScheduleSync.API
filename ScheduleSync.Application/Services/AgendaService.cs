using ScheduleSync.Domain.Entities;
using ScheduleSync.Infrastructure.Repositories;

namespace ScheduleSync.Application.Services
{
    public class AgendaService : IAgendaService
    {
        private readonly IAgendaRepository _scheduleRepository;

        public AgendaService(IAgendaRepository userRepository)
        {
            _scheduleRepository = userRepository;
        }

        public async Task<IEnumerable<AgendaModel>> CreateMultipleSchedulesAsync(List<AgendaModel> agendaModels)
        {
            return await _scheduleRepository.CreateMultipleSchedulesAsync(agendaModels);
        }

        public async Task<AgendaModel> CreateScheduleAsync(AgendaModel agenda)
        {
            try
            {
                return await _scheduleRepository.CreateScheduleAsync(agenda);

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task DeleteScheduleAsync(int id)
        {
            try
            {
                await _scheduleRepository.DeleteScheduleAsync(id);

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<IEnumerable<AgendaModel>> GetScheduleByDoctorIdAsync(int doctorId)
        {
            try
            {

                return await _scheduleRepository.GetScheduleByDoctorIdAsync(doctorId);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<AgendaModel> GetScheduleByIdAsync(int id)
        {
            try
            {
                return await _scheduleRepository.GetScheduleByIdAsync(id);

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task UpdateScheduleAsync(AgendaModel agenda)
        {
            try
            {
                await _scheduleRepository.UpdateScheduleAsync(agenda);

            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}

