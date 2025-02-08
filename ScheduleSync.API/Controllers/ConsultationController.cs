using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScheduleSync.Application.Services;
using ScheduleSync.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using ScheduleSync.Infrastructure.Repositories;
using System.Security.Claims;

namespace ScheduleSync.API.Controllers
{
    [ApiController]
    [Route("api/consultation")]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultasService _consultationService;
        private readonly IAgendaService  _agendaService;

        public ConsultationController(IConsultasService consultationService, IAgendaService agendaService)
        {
            _consultationService = consultationService;
            _agendaService = agendaService;
        }


        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateConsultation([FromBody] RequestCreateConsultasModel request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role).Value;

                if (role != "patient")
                    return Forbid("Somente o perfil paciente poderá marcar consulta.");

                var agendaDados = await _agendaService.GetScheduleDadosByIdAsync(request.AgendaId.Value);
                var agenda = await _agendaService.GetScheduleByIdAsync(request.AgendaId.Value);
                if (agendaDados != null)
                    return BadRequest("Agenda já foi preenchida.");

                if (agenda == null)
                    return BadRequest("Agenda não existe.");

                if (agenda != null && agenda.Disponivel == false)
                    return BadRequest("Agenda não está mais disponível.");

                var consulta = new ConsultaModel { AgendaId = request.AgendaId, Data = agenda.Data, Hora = agenda.HoraInicio,  MedicoId = agenda.MedicoId, Status = "Agendada" , PacienteId = userId };

                var createdConsultation = await _consultationService.CreateConsultationAsync(consulta);
                return Ok(createdConsultation);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize]
        public async Task<IActionResult> GetConsultationsByDoctor(int doctorId)
        {
            try
            {
                var consultations = await _consultationService.GetConsultationsByDoctorIdAsync(doctorId);
                return Ok(consultations);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateConsultation([FromBody] ConsultaModel consulta)
        {
            try
            {
                //  Obtém o ID do médico autenticado
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role).Value;


                //  Verifica se o médico pode editar a consulta
                var existingConsultation = await _consultationService.GetConsultationByIdAsync(consulta.Id.Value);
                if (existingConsultation == null) return NotFound("Consulta não encontrada.");

                if (role == "doctor")
                {
                    if (existingConsultation.MedicoId != userId)
                        return Forbid("Apenas o médico responsável pode editar esta consulta.");
                }

                if (role == "patient")
                {
                    if (existingConsultation.PacienteId != userId)
                        return Forbid("Apenas o próprio paciente pode editar esta consulta.");
                }


                await _consultationService.UpdateConsultationAsync(consulta);
                return Ok(new { message = "Consulta atualizada com sucesso." });
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConsultation(int id, [FromBody] string justificativa)
        {
            try
            {
                //  Obtém o ID do paciente autenticado
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role).Value;


                //  Verifica se o paciente pode cancelar a consulta
                var existingConsultation = await _consultationService.GetConsultationByIdAsync(id);
                if (existingConsultation == null) return NotFound("Consulta não encontrada.");

                if (role == "doctor")
                {
                    if (existingConsultation.MedicoId != userId)
                        return Forbid("Apenas o médico responsável pode cancelar esta consulta.");
                }

                if (role == "patient")
                {
                    if (existingConsultation.PacienteId != userId)
                        return Forbid("Apenas o próprio paciente pode cancelar esta consulta.");
                }
                string acao = role == "patient" ? "Paciente" : "Médico";
                justificativa = $"Consulta Cancelada pelo {acao}, Justificativa: {justificativa}";

                await _consultationService.DeleteConsultationAsync(id, justificativa);
                return Ok(new { message = "Consulta cancelada com sucesso." });
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

            
        }

        [HttpPut("approve/{id}")]
        [Authorize]
        public async Task<IActionResult> ApproveConsultation(int id)
        {
            try
            {

                //  Obtém o ID do paciente autenticado
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role).Value;

                if (role == "patient")
                {
                        return Forbid("Apenas o médico pode aprovar esta consulta.");
                }

                //  Verifica se o médico pode aprovar a consulta
                var existingConsultation = await _consultationService.GetConsultationByIdAsync(id);
                if (existingConsultation == null) return NotFound("Consulta não encontrada.");

                if (existingConsultation.MedicoId != userId)
                    return Forbid("Apenas o médico responsável pode aprovar esta consulta.");


                await _consultationService.ApproveConsultationAsync(id);
                return Ok(new { message = "Consulta aprovada com sucesso." });
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }
    }
}
