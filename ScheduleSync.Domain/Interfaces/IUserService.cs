using ScheduleSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleSync.Domain.Interfaces
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string email, string password);
        Task RegisterAsync(string username, string cpf, string crm, string email, string password, string role);
        Task<User> GetUser(string email);
    }
}

