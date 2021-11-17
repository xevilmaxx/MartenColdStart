using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.Rates
{
    public class RateCalculationDTO
    {
        /// <summary>
        /// Mainly is used for printing useful logs
        /// </summary>
        public long IndividualAmount { get; set; } = 0;
        public long AmountToPay { get; set; } = 0;
        public bool IsCrossDayDetected { get; set; } = false;

        /// <summary>
        /// Current minimal Priority of rule to consider
        /// </summary>
        public int ReachedPriority { get; set; } = 0;

        public override string ToString()
        {
            return $"AmountToPay: {AmountToPay}, IndividualAmount: {IndividualAmount}, IsCrossDayDetected: {IsCrossDayDetected}, ReachedPriority: {ReachedPriority}";
        }

        /// <summary>
        /// Simply Merges passed object to current in correct way.
        /// Mainly Summs Amount and reports all other passed values from previous object.
        /// </summary>
        /// <param name="Data"></param>
        public void MergeWithSimilar(RateCalculationDTO Data)
        {
            this.IndividualAmount = Data.AmountToPay;
            this.AmountToPay += Data.AmountToPay;
            this.ReachedPriority = Data.ReachedPriority;
            //set to true if at least one of 2 is true, or other problems may occur
            this.IsCrossDayDetected = (this.IsCrossDayDetected || Data.IsCrossDayDetected);
        }

    }
}
