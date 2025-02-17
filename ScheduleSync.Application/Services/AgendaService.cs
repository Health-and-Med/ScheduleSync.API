﻿using ScheduleSync.Domain.Entities;
using ScheduleSync.Infrastructure.Repositories;

namespace ScheduleSync.Application.Services
{
    public class AgendaService : IAgendaService
    {
        private readonly IAgendaRepository _scheduleRepository;
        private readonly RabbitMQService  _rabbitMQService;

        public AgendaService(IAgendaRepository userRepository, IConsultasRepository consultasRepository)
        {
            _scheduleRepository = userRepository;
            _rabbitMQService = new RabbitMQService();
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
                var agendaDados = await _scheduleRepository.GetScheduleDadosByIdAsync(id);
                var agenda = await _scheduleRepository.GetScheduleByIdAsync(id);
                await _scheduleRepository.DeleteScheduleAsync(id);
                await NotificarAsync(agenda, "Cancelada", agendaDados);

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
                var agendaDados = await _scheduleRepository.GetScheduleDadosByIdAsync(agenda.Id.Value);
                await _scheduleRepository.UpdateScheduleAsync(agenda);
                string status = agenda.Disponivel == true ? "Aprovada" : "Cancelada";
                await NotificarAsync(agenda, status, agendaDados);
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

