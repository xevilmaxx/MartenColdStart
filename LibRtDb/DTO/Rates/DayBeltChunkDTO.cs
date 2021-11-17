using GenericAPIProtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.Rates
{
    public class DayBeltChunkDTO
    {
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public TimeBelt TimeBelt { get; set; }
    }
}
