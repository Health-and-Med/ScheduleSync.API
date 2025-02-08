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
                var consulta = await _consultasRepository.GetConsultationByIdAsync(id);
                if (consulta == null)
                    throw new Exception("Consulta não existe na base.");

                if (consulta.Status == "Cancelada")
                    throw new Exception("Consulta já foi cancelada.");

                if (consulta.Status == "Aprovada")
                    throw new Exception("Consulta já foi aprovada.");

                var agenda = await _agendaRepository.GetScheduleByIdAsync(consulta.AgendaId.Value);
                await _consultasRepository.ApproveConsultationAsync(id);
                var agendaDados = await _agendaRepository.GetScheduleDadosByIdAsync(consulta.AgendaId.Value);
                await NotificarAsync(agenda, "Consulta Aprovada", agendaDados);
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
                var agenda = await _agendaRepository.GetScheduleByIdAsync(consulta.AgendaId.Value);
                var newconsulta = await _consultasRepository.CreateConsultationAsync(consulta);
                var agendaDados = await _agendaRepository.GetScheduleDadosByIdAsync(consulta.AgendaId.Value);
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
                var consulta = await _consultasRepository.GetConsultationByIdAsync(id);
                if (consulta.Status == "Cancelada")
                    throw new Exception("Consulta já está cancelada.");
                var agendaDados = await _agendaRepository.GetScheduleDadosByIdAsync(consulta.AgendaId.Value);
                var agenda = await _agendaRepository.GetScheduleByIdAsync(consulta.AgendaId.Value);
                await _consultasRepository.DeleteConsultationAsync(id, justificativa);
                await NotificarAsync(agenda, "Consulta Cancelada", agendaDados, justificativa);
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
                var agenda = await _agendaRepository.GetScheduleByIdAsync(consulta.AgendaId.Value);
                await _consultasRepository.UpdateConsultationAsync (consulta);
                var agendaDados = await _agendaRepository.GetScheduleDadosByIdAsync(consulta.AgendaId.Value);
                await NotificarAsync(agenda, "Consulta Editada", agendaDados);

            }
            catch (Exception e)
            {

                throw;
            }
        }

        private async Task NotificarAsync(AgendaModel agenda, string status, AgendaDadosModel agendaDados, string justificativa = null)
        {

            if (agendaDados != null && agendaDados.PacienteId != null && agendaDados.PacienteId > 0)
            {

                // Formata os horários corretamente para exibição
                string horaInicioFormatada = agendaDados.HoraInicio.ToString(@"hh\:mm\:ss");
                string horaFimFormatada = agendaDados.HoraFim.ToString(@"hh\:mm\:ss");

                // Monta a mensagem
                string mensagem = @$"<strong>{status}</strong>:<br>
                     <strong>Nome Médico</strong>: {agendaDados.NomeMedico}<br>
                     <strong>Nome Paciente</strong>: {agendaDados.NomePaciente}<br>
                     <strong>Data</strong>: {agendaDados.Data?.ToString("dd/MM/yyyy") ?? "Data não informada"}<br>
                     <strong>Hora</strong>: {horaInicioFormatada}<br>
                     <strong>Hora Fim</strong>: {horaFimFormatada}<br>
                     <strong>Status</strong>: {status}<br>
                     <strong>Justificativa</strong>: {justificativa}<br>
                    ";



                _rabbitMQService.PublishNewAppointment(agendaDados.MedicoEmail, agendaDados.PacienteEmail, mensagem);
            }
        }
    }
}

