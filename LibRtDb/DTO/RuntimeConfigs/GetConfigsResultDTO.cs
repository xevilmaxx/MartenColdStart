using LibRtDb.DTO.DeviceConfigs;
using LibRtDb.DTO.DynamicKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.RuntimeConfigs
{
    public class GetConfigsResultDTO
    {
        public List<DevConfig> StaticConfigs { get; set; }
        public List<DynamicKey> DynamicKeys { get; set; }
    }
}
