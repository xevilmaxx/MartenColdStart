using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.Rates.Functions
{
    public class ChunkDayInBelts_ResultDTO
    {
        public List<DayBeltChunkDTO> DayBelts { get; set; } = new List<DayBeltChunkDTO>();
        public bool IsCrossDayDetected { get; set; } = false;
    }
}
