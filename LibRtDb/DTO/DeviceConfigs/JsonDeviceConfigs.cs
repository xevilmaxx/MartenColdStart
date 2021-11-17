using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.DeviceConfigs
{
    public class JsonDeviceConfigs
    {
        public long Id { get; set; }
        public int DeviceType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DevConfig> Configs { get; set; }
    }

}
