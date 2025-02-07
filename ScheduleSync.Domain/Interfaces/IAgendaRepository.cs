using ScheduleSync.Domain.Entities;

namespace ScheduleSync.Infrastructure.Repositories
{
    public interface IAgendaRepository
    {
        Task<IEnumerable<AgendaModel>> CreateMultipleSchedulesAsync(List<AgendaModel> agendaModels);
        Task<AgendaModel> CreateScheduleAsync(AgendaModel agenda);
        Task DeleteScheduleAsync(int id);
        Task<IEnumerable<AgendaModel>> GetScheduleByDoctorIdAsync(int doctorId);
        Task<AgendaModel> GetScheduleByIdAsync(int id);
        Task UpdateScheduleAsync(AgendaModel agenda);
        Task<AgendaDadosModel> GetScheduleDadosByIdAsync(int id);
    }
}