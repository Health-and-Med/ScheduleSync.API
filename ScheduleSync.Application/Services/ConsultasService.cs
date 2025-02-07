using ScheduleSync.Domain.Entities;
using ScheduleSync.Infrastructure.Repositories;

namespace ScheduleSync.Application.Services
{
    public class ConsultasService: IConsultasService
    {
        private readonly IConsultasRepository  _consultasRepository;
        private readonly IAgendaRepository   _agendaRepository;
        private readonly RabbitMQService  _rabbitMQService;

        public ConsultasService(IConsultasRepository consultasRepository, IAgendaRepository agendaRepository)
        {
            _consultasRepository = consultasRepository;
            _rabbitMQService = new RabbitMQService();
            _agendaRepository = agendaRepository;
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
                var agenda = await _agendaRepository.GetScheduleByIdAsync(consulta.Id.Value);
                var newconsulta = await _consultasRepository.CreateConsultationAsync(consulta);
                var agendaDados = await _agendaRepository.GetScheduleDadosByIdAsync(consulta.Id.Value);
                await NotificarAsync(agenda, "Consulta Agendada", agendaDados);
                return newconsulta;
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

        private async Task NotificarAsync(AgendaModel agenda, string status, AgendaDadosModel agendaDados)
        {

            if (agendaDados != null && agendaDados.PacienteId != null && agendaDados.PacienteId > 0)
            {

                // Formata os horários corretamente para exibição
                string horaInicioFormatada = agendaDados.HoraInicio.ToString(@"hh\:mm\:ss");
                string horaFimFormatada = agendaDados.HoraFim.ToString(@"hh\:mm\:ss");

                // Monta a mensagem
                string mensagem = @$"Consulta Atualizada:{Environment.NewLine}
                                         Nome Médico: {agendaDados.NomeMedico}{Environment.NewLine}
                                         Nome Paciente: {agendaDados.NomePaciente}{Environment.NewLine}
                                         Data: {agendaDados.Data?.ToString("dd/MM/yyyy") ?? "Data não informada"}{Environment.NewLine}
                                         Hora: {horaInicioFormatada}{Environment.NewLine}
                                         Hora Fim: {horaFimFormatada}{Environment.NewLine}
                                         Status: {status}";


                _rabbitMQService.PublishNewAppointment(agendaDados.MedicoEmail, agendaDados.PacienteEmail, mensagem);
            }
        }
    }
}

