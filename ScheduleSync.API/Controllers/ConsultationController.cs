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

        public ConsultationController(IConsultasService consultationService)
        {
            _consultationService = consultationService;
        }


        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateConsultation([FromBody] ConsultaModel consulta)
        {
            var createdConsultation = await _consultationService.CreateConsultationAsync(consulta);
            return Ok(createdConsultation);
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize]
        public async Task<IActionResult> GetConsultationsByDoctor(int doctorId)
        {
            var consultations = await _consultationService.GetConsultationsByDoctorIdAsync(doctorId);
            return Ok(consultations);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateConsultation([FromBody] ConsultaModel consulta)
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

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConsultation(int id, [FromBody] string justificativa)
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


            await _consultationService.DeleteConsultationAsync(id, justificativa);
            return Ok(new { message = "Consulta cancelada com sucesso." });
        }

        [HttpPut("approve/{id}")]
        [Authorize]
        public async Task<IActionResult> ApproveConsultation(int id)
        {
            //  Obtém o ID do médico autenticado
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            //  Verifica se o médico pode aprovar a consulta
            var existingConsultation = await _consultationService.GetConsultationByIdAsync(id);
            if (existingConsultation == null) return NotFound("Consulta não encontrada.");

            if (existingConsultation.MedicoId != userId)
                return Forbid("Apenas o médico responsável pode aprovar esta consulta.");


            await _consultationService.ApproveConsultationAsync(id);
            return Ok(new { message = "Consulta aprovada com sucesso." });
        }
    }
}
