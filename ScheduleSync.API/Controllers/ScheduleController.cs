using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScheduleSync.Application.Services;
using ScheduleSync.Domain.Entities;
using ScheduleSync.Infrastructure.Repositories;

namespace ScheduleSync.API.Controllers
{
    [ApiController]
    [Route("api/schedule")]
    public class ScheduleController : ControllerBase
    {
        private readonly IAgendaService _scheduleService;

        public ScheduleController(IAgendaService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateSchedule([FromBody] RequestCreateAgendaModel agenda)
        {
            try
            {
                //  Obtém o ID do médico autenticado
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role).Value;

                if (role != "doctor")
                    return Forbid("Apenas perfil médico pode criar uma agenda.");

                AgendaModel agendaModel = new AgendaModel { MedicoId = userId, Data = agenda.Data, Disponivel = agenda.Disponivel, HoraFim = agenda.HoraFim, HoraInicio = agenda.HoraInicio, PrecoConsulta = agenda.PrecoConsulta };

                var createdSchedule = await _scheduleService.CreateScheduleAsync(agendaModel);
                return Ok(createdSchedule);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }

        [HttpPost("create-list")]
        [Authorize]
        public async Task<IActionResult> CreateSchedules([FromBody] List<RequestCreateAgendaModel> agendas)
        {
            try
            {
                // 🔹 Obtém o ID do médico autenticado
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (role != "doctor")
                    return Forbid("Apenas médicos podem criar uma agenda.");

                // 🔹 Converte os modelos de request para entidades de agenda
                List<AgendaModel> agendaModels = agendas.Select(agenda => new AgendaModel
                {
                    MedicoId = userId,
                    Data = agenda.Data,
                    Disponivel = agenda.Disponivel,
                    HoraFim = agenda.HoraFim,
                    HoraInicio = agenda.HoraInicio,
                    PrecoConsulta = agenda.PrecoConsulta
                }).ToList();

                // 🔹 Chama o serviço para inserir os registros
                var createdSchedules = await _scheduleService.CreateMultipleSchedulesAsync(agendaModels);

                return Ok(createdSchedules);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }


        [HttpGet("doctor/{doctorId}")]
        [Authorize]
        public async Task<IActionResult> GetSchedulesByDoctor(int doctorId)
        {
            try
            {
                var schedules = await _scheduleService.GetScheduleByDoctorIdAsync(doctorId);
                return Ok(schedules);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
           
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateSchedule([FromBody] RequestUpdateAgendaModel agenda)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role).Value;

                if (role != "doctor")
                    return BadRequest("Apenas perfil médico pode atualizar agenda.");

                AgendaModel agendaModel = await _scheduleService.GetScheduleByIdAsync(agenda.Id.Value);

                if (agendaModel == null)
                    return NotFound("Agenda não encontrada.");

                if (userId != agendaModel.MedicoId)
                    return Forbid("Apenas o médico responsável pode atualizar a própria agenda.");
                AgendaModel newAgenda = new AgendaModel { Id = agenda.Id, Data = agenda.Data, Disponivel = agenda.Disponivel, HoraFim = agenda.HoraFim, HoraInicio = agenda.HoraInicio, PrecoConsulta = agenda.PrecoConsulta };



                await _scheduleService.UpdateScheduleAsync(newAgenda);
                return Ok(new { message = "Agenda atualizada com sucesso." });
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

            
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role).Value;

                AgendaModel agenda = await _scheduleService.GetScheduleByIdAsync(id);

                if (agenda == null)
                    return NotFound("Agenda não encontrada.");

                if (role != "doctor")
                    return Forbid("Apenas perfil médico pode atualizar agenda.");

                if (userId != agenda.MedicoId)
                   return Forbid("Apenas o médico responsável pode deletar a própria agenda.");

                await _scheduleService.DeleteScheduleAsync(id);
                return Ok(new { message = "Agenda excluída com sucesso." });
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }

        [HttpGet("schedule/{scheduleId}")]
        [Authorize]
        public async Task<IActionResult> GetScheduleByIdAsync(int scheduleId)
        {
            try
            {
                var schedules = await _scheduleService.GetScheduleByIdAsync(scheduleId);
                return Ok(schedules);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }
    }
}
