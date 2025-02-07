using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleSync.Domain.Entities
{
    public class ConsultaModel
    {
        public int? Id { get; set; }
        public int? PacienteId { get; set; }
        public int? MedicoId { get; set; }
        public int? AgendaId { get; set; }
        public DateTime? Data { get; set; } // Apenas a data, sem hora
        public TimeSpan? Hora { get; set; } // Apenas a hora
        public string Status { get; set; } // Status da consulta (Ex: Agendada, Cancelada, Aprovada)
        public string JustificativaCancelamento { get; set; } // Justificativa opcional para cancelamento
    }
}
