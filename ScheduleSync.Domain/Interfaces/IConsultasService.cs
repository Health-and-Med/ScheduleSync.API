using ScheduleSync.Domain.Entities;

namespace ScheduleSync.Infrastructure.Repositories
{
    public interface IConsultasService
    {
        Task ApproveConsultationAsync(int id);
        Task<ConsultaModel> CreateConsultationAsync(ConsultaModel consulta);
        Task DeleteConsultationAsync(int id, string justificativa);
        Task<ConsultaModel> GetConsultationByIdAsync(int id);
        Task<IEnumerable<ConsultaModel>> GetConsultationsByDoctorIdAsync(int doctorId);
        Task UpdateConsultationAsync(ConsultaModel consulta);
    }
}