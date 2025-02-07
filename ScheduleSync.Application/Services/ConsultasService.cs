using ScheduleSync.Domain.Entities;
using ScheduleSync.Infrastructure.Repositories;

namespace ScheduleSync.Application.Services
{
    public class ConsultasService: IConsultasService
    {
        private readonly IConsultasRepository  _consultasRepository;

        public ConsultasService(IConsultasRepository consultasRepository)
        {
            _consultasRepository = consultasRepository;
        }

        public async Task ApproveConsultationAsync(int id)
        {
            try
            {
                await _consultasRepository.ApproveConsultationAsync(id);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<ConsultaModel> CreateConsultationAsync(ConsultaModel consulta)
        {
            try
            {
                return await _consultasRepository.CreateConsultationAsync(consulta);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task DeleteConsultationAsync(int id, string justificativa)
        {
            try
            {
                await _consultasRepository.DeleteConsultationAsync(id, justificativa);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<ConsultaModel> GetConsultationByIdAsync(int id)
        {
            try
            {
                return await _consultasRepository.GetConsultationByIdAsync(id);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<IEnumerable<ConsultaModel>> GetConsultationsByDoctorIdAsync(int doctorId)
        {
            try
            {
                return await _consultasRepository.GetConsultationsByDoctorIdAsync(doctorId);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task UpdateConsultationAsync(ConsultaModel consulta)
        {
            try
            {
                await _consultasRepository.UpdateConsultationAsync (consulta);
            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}

