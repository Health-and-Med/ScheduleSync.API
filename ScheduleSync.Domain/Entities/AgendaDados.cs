using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScheduleSync.Domain.Entities
{
    public class AgendaDadosModel
    {
        public int? Id { get; set; }
        public int? ConsultaId { get; set; }
        public string NomePaciente { get; set; }
        public string NomeMedico { get; set; }
        public string PacienteEmail { get; set; }
        public string MedicoEmail { get; set; }
        public int? MedicoId { get; set; }
        public int? PacienteId { get; set; }
        public DateTime? Data { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public bool? Disponivel { get; set; }
        public decimal? PrecoConsulta { get; set; }
    }
}
