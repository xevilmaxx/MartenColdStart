using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.Rates.Functions
{
    public class ConsumeTimeBeltDTO
    {
        public DayBeltChunkDTO BeltChunk { get; set; }
        public int StartingRuleIndex { get; set; }
        public RateCalculationDTO RateCalc { get; set; }
        public IterationPosEnum DayIterPosEnum { get; set; }
        public IterationPosEnum BeltIterPosEnum { get; set; }
    }
}
