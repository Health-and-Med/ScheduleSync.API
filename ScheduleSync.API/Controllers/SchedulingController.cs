using ScheduleSync.Domain.Entities;
using ScheduleSync.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ScheduleSync.Application.Services;

namespace ScheduleSync.API.Controllers
{
    [ApiController]
    [Route("api/scheduling")]
    public class SchedulingController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IScheduleService _scheduleService;
        private readonly RabbitMQService _rabbitMQService;

        public SchedulingController(IConfiguration configuration, IScheduleService userService, RabbitMQService rabbitMQService)
        {
            _configuration = configuration;
            _scheduleService = userService;
            _rabbitMQService = rabbitMQService;
        }

        [Authorize]
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleAppointment([FromBody] ScheduleAppointmentRequest request)
        {
            var exists = await _scheduleService.GetSheduleDoctor(request.DoctorId, request.ScheduledDate);

            if (exists != null)
                return BadRequest("Horário já agendado!");

            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                ScheduledDate = request.ScheduledDate
            };

            await _scheduleService.AddScheduleAsync(request);

            // Enviar notificação para o RabbitMQ

            _rabbitMQService.PublishNewAppointment("matheusfonsecamfo@gmail.com","matheus","maria", "20/02/2025","13:00:55");

            return Ok("Consulta agendada com sucesso!");
        }

        [HttpGet("ping")]
        [Authorize]
        public IActionResult Ping()
        {
            return Ok(new { message = "Pong! Você está autenticado." });
        }
    }
}

