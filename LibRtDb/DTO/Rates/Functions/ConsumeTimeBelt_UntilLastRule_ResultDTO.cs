using GenericAPIProtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.Rates.Functions
{
    public class ConsumeTimeBelt_UntilLastRule_ResultDTO
    {
        public bool IsNeedStop { get; set; }
        public double GapToFill { get; set; }
        public long AmountToPay { get; set; }
        public PaymentRule LastAppliedRule { get; set; }
    }
}
