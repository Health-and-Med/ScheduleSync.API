using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleSync.Domain.Entities
{
    public class ScheduleAppointmentRequest
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime ScheduledDate { get; set; }
    }
}
