using LibRtDb.DTO.DeviceConfigs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Services
{
    public class FileConfigsService
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns Config from File
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <returns></returns>
        public (JsonDeviceConfigs configs, JObject originalFile) GetConfigs(long DeviceId)
        {
            try
            {

                log.Debug($"GetConfigs Invoked! DevceId: {DeviceId}");

                JsonDeviceConfigs result = null;

                //read from file
                JObject data = JObject.Parse(File.ReadAllText(@"AppSettings.json"));

                result = data["Config"].ToObject<JsonDeviceConfigs>();

                return (result, data);

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return (new JsonDeviceConfigs(), null);
            }
        }

        /// <summary>
        /// Writes configs to file if there have benn changes
        /// </summary>
        /// <param name="Configs"></param>
        /// <param name="originalFile"></param>
        /// <returns></returns>
        public bool SetConfigs(JsonDeviceConfigs Configs, JObject originalFile = null)
        {
            try
            {

                log.Debug("SetConfigs Invoked!");

                if (Configs == null)
                {
                    log.Debug("No configs passed");
                    return true;
                }

                bool result = false;

                string FilePath = @"AppSettings.json";

                if (originalFile == null)
                {
                    //re read data, inefficient but ok for now
                    originalFile = JObject.Parse(File.ReadAllText(FilePath));
                }

                var origCfg = originalFile["Config"];
                var newCfg = JToken.FromObject(Configs);

                if(JToken.DeepEquals(origCfg, newCfg) == false)
                {
                    //persist back to disk
                    string json = JsonConvert.SerializeObject(originalFile, Formatting.Indented);
                    //write string to file
                    System.IO.File.WriteAllText(FilePath, json);
                }

                result = true;

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

    }
}
