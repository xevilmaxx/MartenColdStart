using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.DynamicKeys
{
    public class DynamicKey
    {
        public long Id { get; set; }
        public long DeviceId { get; set; }
        public int DeviceType { get; set; }
        public string Key { get; set; }
        public dynamic Value { get; set; }
        public string Description { get; set; }
    }
}
