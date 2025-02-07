using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScheduleSync.Domain.Entities
{
    public class RequestCreateAgendaModel
    {
        public DateTime? Data { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public bool? Disponivel { get; set; }
        public decimal? PrecoConsulta { get; set; }
    }
}
