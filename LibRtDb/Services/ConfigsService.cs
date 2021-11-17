using LibRtDb.DTO.DeviceConfigs;
using LibRtDb.DTO.DynamicKeys;
using Marten;
using Marten.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Services
{
    public class ConfigsService
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public DyncamicKeysService DKS { get; private set; } = new DyncamicKeysService();

        #region Read

        /// <summary>
        /// Returns specific configs based on Id
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <returns></returns>
        public JsonDeviceConfigs GetConfigs(long DeviceId)
        {
            try
            {

                log.Debug($"GetConfigs Invoked! DevceId: {DeviceId}");

                JsonDeviceConfigs result = null;

                using (var session = DbFactory.GetContext().QuerySession())
                {
                    //just for debugging
                    //var tmp = session.Query<JsonDeviceConfigs>().ToList();

                    //get somthing, if nothing return new of the config object
                    result = session.Query<JsonDeviceConfigs>()
                        .Where(x => x.Id == DeviceId)
                        .FirstOrDefault()
                        ??
                        new JsonDeviceConfigs();
                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new JsonDeviceConfigs();
            }
        }

        /// <summary>
        /// Returns configs that Matches specific Type
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <returns></returns>
        public List<JsonDeviceConfigs> GetConfigs(int DeviceType)
        {
            try
            {

                log.Debug($"GetConfigs Invoked! DeviceType: {DeviceType}");

                List<JsonDeviceConfigs> result = null;

                using (var session = DbFactory.GetContext().QuerySession())
                {
                    //just for debugging
                    //var tmp = session.Query<JsonDeviceConfigs>().ToList();

                    //get somthing, if nothing return new of the config object
                    result = session.Query<JsonDeviceConfigs>()
                        .Where(x => x.DeviceType == DeviceType)
                        .ToList()
                        ??
                        new List<JsonDeviceConfigs>();
                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new List<JsonDeviceConfigs>();
            }
        }

        /// <summary>
        /// Returns all configs in DB
        /// </summary>
        /// <returns></returns>
        public List<JsonDeviceConfigs> GetConfigs()
        {
            try
            {

                log.Debug("GetConfigs Invoked!");

                List<JsonDeviceConfigs> result = null;

                using (var session = DbFactory.GetContext().QuerySession())
                {
                    //just for debugging
                    //var tmp = session.Query<JsonDeviceConfigs>().ToList();

                    //get somthing, if nothing return new of the config object
                    result = session.Query<JsonDeviceConfigs>().ToList() ?? new List<JsonDeviceConfigs>();
                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new List<JsonDeviceConfigs>();
            }
        }

        public DevConfig GetSpecificKeyValue(long DeviceId, string Key)
        {
            try
            {

                DevConfig res = null;

                using (var session = DbFactory.GetContext().QuerySession())
                {
                    log.Debug("Searching for key: " + Key);

                    //first of all lets check if its one of dynamic key (they are few)
                    var isOneOfDynamics = session.Query<DynamicKey>().Where(x => x.Key.Equals(Key)).FirstOrDefault();
                    if (isOneOfDynamics != null)
                    {
                        //lets return the key
                        res = new DevConfig()
                        {
                            Key = isOneOfDynamics.Key,
                            Value = isOneOfDynamics.Value,
                            Description = isOneOfDynamics.Description
                        };
                    }
                    else
                    {
                        //lets search also in static configs, if we hadnt succeded before
                        res = (
                            from devConfigs in session.Query<JsonDeviceConfigs>()
                            where devConfigs.Id == DeviceId
                            from devConfig in devConfigs.Configs
                            where devConfig.Key.Equals(Key)
                            select devConfig
                        ).FirstOrDefault();
                    }

                    if (res == null)
                    {
                        log.Debug("For device: " + DeviceId + ", No such key found: " + Key);
                    }

                    //Alternative way (Working ust if needed)
                    //var res2 = session.Query<JsonDeviceConfigs>()
                    //    .Where(x => x.Id == (long)DeviceType.ParkO_RTS)
                    //    .SelectMany(a => a.Configs, (a, b) => b)
                    //    .Where(c => c.Key.Equals("LocalServerPort", StringComparison.InvariantCultureIgnoreCase))
                    //    .FirstOrDefault();

                }

                return res;

            }
            catch (Exception ex)
            {
                log.Debug(ex, "GetSpecificKeyValue -> ");
                return null;
            }
        }

        /// <summary>
        /// Get all available Devices
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetDeviceList()
        {
            try
            {
                log.Debug("Getting list of all devices");
                Dictionary<string, string> result = new Dictionary<string, string>();

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    var res = (
                        from dev in session.Query<JsonDeviceConfigs>()
                        select new { dev.Id, dev.Name }
                        ).ToList();


                    foreach (var r in res)
                    {
                        result.Add(r.Id.ToString(), r.Name);
                    }
                }

                return result;

            }
            catch (Exception ex)
            {
                log.Debug(ex);
                return null;
            }
        }

        /// <summary>
        /// Gets Local Core GRPC port number from DB
        /// </summary>
        /// <returns></returns>
        public string GetCoreListeningPort()
        {
            try
            {

                var coreConfigs = GetConfigs((long)1);
                
                string result = GetCoreListeningPort(coreConfigs);

                return result;

            }
            catch (Exception ex)
            {
                log.Debug(ex);
                return null;
            }
        }

        #endregion

        #region Write

        public bool SetConfigs(List<JsonDeviceConfigs> Configs)
        {
            try
            {

                log.Debug("SetConfigs Invoked!");

                if (Configs == null || Configs.Count == 0)
                {
                    log.Debug("No configs passed");
                    return true;
                }

                bool result = false;

                using (var session = DbFactory.GetContext().LightweightSession())
                {
                    //get somthing, if nothing return new of the config object
                    session.Store<JsonDeviceConfigs>(Configs);
                    session.SaveChanges();
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

        private void GenericKeyUpsert(IDocumentSession session, long DeviceId, string Key, dynamic Value)
        {
            var result = session.Query<DynamicKey>().Where(x => x.Key.Equals(Key)).FirstOrDefault();
            if (result == null)
            {
                //lets go for static settings
                var device = session.Query<JsonDeviceConfigs>().Where(x => x.Id.Equals(DeviceId)).FirstOrDefault();
                var deviceConfig = device?.Configs.Where(x => x.Key.Equals(Key)).FirstOrDefault();

                if (device == null && deviceConfig == null)
                {
                    log.Debug("Will generate object completely");
                    device = new JsonDeviceConfigs()
                    {
                        Id = DeviceId,
                        DeviceType = 0,
                        Description = "Generated",
                        Name = "Generated",
                        Configs = new List<DevConfig>()
                        {
                            new DevConfig()
                            {
                                Key = Key,
                                Value = Value,
                                Description = "Generated"
                            }
                        }
                    };
                }
                else if (device != null && deviceConfig == null)
                {
                    log.Debug("Will add just new config");
                    //even here nothing, but if its new key its better to insert it here
                    device.Configs.Add(new DevConfig()
                    {
                        Key = Key,
                        Value = Value,
                        Description = "Generated"
                    });
                }
                else
                {
                    log.Debug("Will just update static config");
                    //wow we found something in here
                    deviceConfig.Value = Value;
                }

                session.Store(device);

            }
            else
            {
                result.Value = Value;
                session.Store(result);
            }
        }

        private void StaticKeyUpsert(IDocumentSession session, long DeviceId, string Key, dynamic Value)
        {
            var device = session.Query<JsonDeviceConfigs>().Where(x => x.Id == DeviceId).FirstOrDefault();
            var deviceConfig = device?.Configs.Where(x => x.Key.Equals(Key)).FirstOrDefault();

            if (device == null && deviceConfig == null)
            {
                log.Debug("Will generate object completely");
                //lets create object completely
                device = new JsonDeviceConfigs()
                {
                    Id = DeviceId,
                    DeviceType = 0,
                    Description = "AutoGenerated",
                    Name = "AutoGenerated",
                    Configs = new List<DevConfig>()
                                {
                                    new DevConfig()
                                    {
                                        Key = Key,
                                        Value = Value,
                                        Description = "AutoGenerated"
                                    }
                                }
                };
            }
            else if (deviceConfig == null)
            {
                //If we miss just config
                log.Debug("For device: " + DeviceId + ", No such key found: " + Key);
                log.Debug("The key: " + Key + " will be added!");

                var newKey = new DevConfig();
                newKey.Key = Key;
                newKey.Value = Value;
                newKey.Description = "!GENERATED!";

                //its missing key so we can add it to existing object
                device.Configs.Add(newKey);

            }
            else
            {
                //we found also key
                deviceConfig.Value = Value;
                log.Debug("Key: " + Key + ", will be updated");
            }

            //Here i avoid auto change tracking for performance, but its doable 
            session.Store(device);
        }

        private void GlobalGenericKeyUpsertLogic(IDocumentSession session, long DeviceId, string Key, dynamic Value, bool? IsDynamicKey = null)
        {
            log.Debug("Changing key: " + Key + ", with value: " + Value);

            //needs rebuilds for postgres on RPI for now
            //session.Patch<JsonDeviceConfigs>(x => x.Id == DeviceId).Set(x => x.Configs.Where(x=>x.Key == "").FirstOrDefault().Value, -1);

            if (IsDynamicKey == null)
            {
                //we dont know what key type it is, static or dynamic
                //so lets start from dynamics that are few
                GenericKeyUpsert(session, DeviceId, Key, Value);
            }
            else if (IsDynamicKey == true)
            {
                log.Debug("Will set Dynamic Key");
                DKS.Set(session, DeviceId, Key, Value);
            }
            else
            {
                StaticKeyUpsert(session, DeviceId, Key, Value);
            }
        }

        /// <summary>
        /// Upsers list of values
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <param name="Values"></param>
        /// <param name="IsDynamicKey"></param>
        /// <returns></returns>
        public bool SetSpecificKeyValue(long DeviceId, Dictionary<string, dynamic> Values, bool? IsDynamicKey = null)
        {
            try
            {

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    foreach (var val in Values)
                    {
                        GlobalGenericKeyUpsertLogic(session, DeviceId, val.Key, val.Value, IsDynamicKey);
                    }

                    session.SaveChanges();

                }

                return true;

            }
            catch (Exception ex)
            {
                log.Debug(ex, "SetSpecificKeyValue -> ");
                return false;
            }
        }

        /// <summary>
        /// Upserts single value
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="IsDynamicKey"></param>
        /// <returns></returns>
        public bool SetSpecificKeyValue(long DeviceId, string Key, dynamic Value, bool? IsDynamicKey = null)
        {
            try
            {

                using (var session = DbFactory.GetContext().LightweightSession())
                {
                    GlobalGenericKeyUpsertLogic(session, DeviceId, Key, Value, IsDynamicKey);
                    session.SaveChanges();
                }

                return true;

            }
            catch (Exception ex)
            {
                log.Debug(ex, "SetSpecificKeyValue -> ");
                return false;
            }
        }

        /// <summary>
        /// Efficiently updates static configs
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <param name="Values"></param>
        /// <returns></returns>
        public bool StaticConfigUpsert(long DeviceId, Dictionary<string, dynamic> Values)
        {
            try
            {

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    var staticConfig = session.Query<JsonDeviceConfigs>().Where(x => x.Id.Equals(DeviceId)).FirstOrDefault();
                    if (staticConfig == null)
                    {
                        return false;
                    }

                    foreach (var val in Values)
                    {

                        var existentKey = staticConfig.Configs.Where(x => x.Key.Equals(val.Key)).FirstOrDefault();
                        if (existentKey == null)
                        {
                            //lets add
                            staticConfig.Configs.Add(new DevConfig()
                            {
                                Key = val.Key,
                                Value = val.Value,
                                Description = "Generated"
                            });
                        }
                        else
                        {
                            //lets change
                            existentKey.Value = val.Value;
                        }

                    }

                    session.Store(staticConfig);

                    session.SaveChanges();

                }

                return true;

            }
            catch (Exception ex)
            {
                log.Debug(ex, "SetSpecificKeyValue -> ");
                return false;
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Not real query on DB
        /// </summary>
        /// <param name="CoreConfigs"></param>
        /// <returns></returns>
        public string GetCoreListeningPort(JsonDeviceConfigs CoreConfigs)
        {
            try
            {

                return CoreConfigs.Configs.Where(x => x.Key == "listening_port").FirstOrDefault().Value.ToString();

            }
            catch (Exception ex)
            {
                log.Debug(ex);
                return null;
            }
        }

        #endregion

        #region Delete
        #endregion

    }
}
