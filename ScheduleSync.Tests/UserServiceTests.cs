using ScheduleSync.Application.Services;
using ScheduleSync.Domain.Entities;
using ScheduleSync.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ScheduleSync.Infrastructure.Repositories;

namespace ScheduleSync.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IAgendaRepository> _mockContactRepository;
        private readonly IAgendaService _userService;


        public UserServiceTests()
        {
            _mockContactRepository = new Mock<IAgendaRepository>();
            _userService = new AgendaService(_mockContactRepository.Object);
        }

        //[Fact]
        //public async Task AddUser_ShouldThrowValidationException_WhenEmailIsEmpty()
        //{
        //    // Arrange
        //    var region = new User { Username = "Teste", Email = "", Cpf = "00000", Role = "user", PasswordHash = "5454", Crm = "" };

        //    // Act & Assert
        //    await Assert.ThrowsAsync<ValidationException>(() => _userService.RegisterAsync(region.Username, region.Cpf, region.Crm, region.Email, region.PasswordHash, region.Role));
        //}
    }
}

