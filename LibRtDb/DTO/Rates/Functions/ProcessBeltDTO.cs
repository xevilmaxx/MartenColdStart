using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.Rates.Functions
{
    public class ProcessBeltDTO
    {
        public DayBeltChunkDTO BeltChunk { get; set; }
        public IterationPosEnum DayIterPosEnum { get; set; }
        public IterationPosEnum BeltIterPosEnum { get; set; }
        public int PreviouslyReachedPriority { get; set; }
    }
}
