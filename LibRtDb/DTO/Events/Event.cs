using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.Events
{
    public class Event
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public long DeviceId { get; set; }
        public int EventType { get; set; }
    }
}
