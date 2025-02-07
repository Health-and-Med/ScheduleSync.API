using ScheduleSync.Domain.Entities;
using ScheduleSync.Infrastructure.Repositories;

namespace ScheduleSync.Application.Services
{
    public class AgendaService : IAgendaService
    {
        private readonly IAgendaRepository _scheduleRepository;
        private readonly IConsultasRepository  _consultasRepository;

        public AgendaService(IAgendaRepository userRepository, IConsultasRepository consultasRepository)
        {
            _scheduleRepository = userRepository;
            _consultasRepository = consultasRepository;
        }

        public async Task<IEnumerable<AgendaModel>> CreateMultipleSchedulesAsync(List<AgendaModel> agendaModels)
        {

            List<string> erros = new List<string>();

            foreach (var agenda in agendaModels)
            {
                var agendaExist = await _scheduleRepository.GetScheduleByDoctorIdAsync(agenda.MedicoId.Value);
                // Verifica se há conflitos com a agenda
                bool exist = agendaExist.Any(x =>
                    x.Data.Value.ToString("yyyyMMdd") == agenda.Data.Value.ToString("yyyyMMdd") &&
                    (
                        (agenda.HoraInicio >= x.HoraInicio && agenda.HoraInicio < x.HoraFim) ||
                        (agenda.HoraFim > x.HoraInicio && agenda.HoraFim <= x.HoraFim) ||
                        (agenda.HoraInicio <= x.HoraInicio && agenda.HoraFim >= x.HoraFim)
                    )
                );

                if (exist)
                    erros.Add($"Agenda já existe ou intersecta com agenda já existente: \n Data: {agenda.Data.Value:dd/MM/yyyy} Hora Inicio:{agenda.HoraInicio} Hora Final:{agenda.HoraFim} ");

            }
            if (erros.Count > 0)
                throw new Exception(string.Join("\n", erros));


            return await _scheduleRepository.CreateMultipleSchedulesAsync(agendaModels);
        }

        public async Task<AgendaModel> CreateScheduleAsync(AgendaModel agenda)
        {
            try
            {
                var agendaExist = await _scheduleRepository.GetScheduleByDoctorIdAsync(agenda.MedicoId.Value);
                // Verifica se há conflitos com a agenda
                bool exist = agendaExist.Any(x =>
                    x.Data.Value.ToString("yyyyMMdd") == agenda.Data.Value.ToString("yyyyMMdd") &&
                    (
                        (agenda.HoraInicio >= x.HoraInicio && agenda.HoraInicio < x.HoraFim) ||
                        (agenda.HoraFim > x.HoraInicio && agenda.HoraFim <= x.HoraFim) ||
                        (agenda.HoraInicio <= x.HoraInicio && agenda.HoraFim >= x.HoraFim)
                    )
                );

                if (exist)
                    throw new Exception($"Agenda já existe ou intersecta com agenda já existente: \n Data: {agenda.Data.Value:dd/MM/yyyy} Hora Inicio:{agenda.HoraInicio} Hora Final:{agenda.HoraFim} ");

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

        public Task<AgendaDadosModel> GetScheduleDadosByIdAsync(int id)
        {
            try
            {
                return _scheduleRepository.GetScheduleDadosByIdAsync(id);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public async Task UpdateScheduleAsync(AgendaModel agenda)
        {
            try
            {




                await _scheduleRepository.UpdateScheduleAsync(agenda);
                var agendaDados = await _scheduleRepository.GetScheduleDadosByIdAsync(agenda.Id.Value);

                if (agendaDados != null && agendaDados.PacienteId != null && agendaDados.PacienteId > 0)
                {
                    string status = agendaDados.Disponivel == true ? "Aprovada" : "Cancelada";
                    string mensagem = @$"Consulta Atualizada:
                                        Nome Médico: {agendaDados.NomeMedico}
                                        Nome Paciente: {agendaDados.NomePaciente}
                                        Data: {agendaDados.Data.Value.ToString("dd/MM/yyyy")}
                                        Hora: {agendaDados.HoraInicio.ToString("HH:mm")}
                                        Hora Fim: {agendaDados.HoraFim.ToString("HH:mm")}
                                        Status: {status}
                                        ";
                    // Enviar notificação para o RabbitMQ
                    var rabbitMQService = new RabbitMQService();
                    rabbitMQService.PublishNewAppointment(agendaDados.PacienteEmail, agendaDados.PacienteEmail, mensagem);
                }

            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}

