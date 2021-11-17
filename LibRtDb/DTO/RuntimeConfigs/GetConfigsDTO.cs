using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.RuntimeConfigs
{
    public class GetConfigsDTO
    {
        public long DeviceId { get; set; }
        public int DeviceType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
