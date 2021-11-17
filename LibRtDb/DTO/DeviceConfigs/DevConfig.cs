using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.DeviceConfigs
{
    public class DevConfig
    {
        public string Key { get; set; }

        //This is important to be dynamic, becouse we can even make system more efficient if we specify
        //Correct data type during registration
        public dynamic Value { get; set; }

        public string Description { get; set; }
    }
}
