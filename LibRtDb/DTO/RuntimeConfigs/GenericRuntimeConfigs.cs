using LibRtDb.DTO.DeviceConfigs;
using LibRtDb.DTO.DynamicKeys;
using LibRtDb.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.RuntimeConfigs
{
    public static class GenericRuntimeConfigs
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static ConfigsService cfgService = new ConfigsService();

        /// <summary>
        /// Collects all Current class properties into special structure
        /// Considers data annotaions, but it should be null safe
        /// </summary>
        /// <returns></returns>
        public static GetConfigsResultDTO GetConfigs(Type T, GetConfigsDTO data)
        {
            var resultStatic = new List<DevConfig>();
            var resultDynamic = new List<DynamicKey>();

            //Cycle on all properties of configuration class
            PropertyInfo[] properties = T.GetProperties();
            foreach (PropertyInfo property in properties)
            {

                var annotations = property?
                    .CustomAttributes?
                    .FirstOrDefault()?
                    .NamedArguments?
                    .ToList();

                var nameOnDb = annotations?.Where(x => x.MemberName.Equals("Name")).FirstOrDefault().TypedValue.Value?.ToString();
                //becouse we dont want eventual errors of programmer, name cannot be empty!
                if(string.IsNullOrEmpty(nameOnDb) || string.IsNullOrWhiteSpace(nameOnDb))
                {
                    if(nameOnDb != null) { nameOnDb = null; }
                }

                var description = annotations?.
                    Where(x => x.MemberName.Equals("Description")).FirstOrDefault().TypedValue.Value?.ToString()
                    ??
                    "Auto_Generated"
                    ;

                //Write always at least if its explicitly not required
                bool isNeedWrite  = (bool?)annotations?.
                    Where(x => x.MemberName.Equals("AutoGenerateField")).FirstOrDefault().TypedValue.Value
                    ??
                    true
                    ;

                //dont write it to DB, just skip
                if (isNeedWrite == false)
                {
                    continue;
                }

                var dynProp = annotations?.Where(x => x.MemberName.Equals("GroupName")).FirstOrDefault().TypedValue.Value?.ToString();
                if(dynProp != null && dynProp.Equals("DYNAMIC"))
                {
                    resultDynamic.Add(new DynamicKey()
                    {
                        DeviceId = data.DeviceId,
                        DeviceType = data.DeviceType,
                        Key = nameOnDb ?? property.Name,
                        Value = property.GetValue(property),
                        Description = description
                    });
                }
                else
                {
                    resultStatic.Add(new DevConfig()
                    {
                        Key = nameOnDb ?? property.Name,
                        Value = property.GetValue(property),
                        Description = description
                    });
                }

            }
            return new GetConfigsResultDTO() {
                StaticConfigs = resultStatic,
                DynamicKeys = resultDynamic
            };
        }

        /// <summary>
        /// Dynamically recreates all needed keys on DB
        /// Resulting Configs will be populated by reference
        /// </summary>
        /// <param name="configs"></param>
        public static void CreateNeededVarsInDB(Type T, bool IsFirstBoot, ref JsonDeviceConfigs configs, GetConfigsDTO data)
        {

            GetConfigsResultDTO freshAutogenConfigs = null;
            if (configs == null || configs.Configs == null)
            {
                log.Debug("No configs found!");
                //var xxx = T.GetMethod("GetConfigs").Invoke(T, new object[] { data });
                freshAutogenConfigs = GetConfigs(T, data);

                if (log.IsTraceEnabled)
                {
                    foreach(var acfg in freshAutogenConfigs.StaticConfigs)
                    {
                        log.Trace($"Fresh Static conf: [{acfg.Key} -> {acfg.Value}]");
                    }
                    foreach (var dcfg in freshAutogenConfigs.DynamicKeys)
                    {
                        log.Trace($"Fresh Dynamic conf: [{dcfg.Key} -> {dcfg.Value}]");
                    }
                }

                //compose basic configuration based on default values
                var basicConfigs = new List<JsonDeviceConfigs>()
                    {
                        new JsonDeviceConfigs() {
                            Id = data.DeviceId,
                            DeviceType = data.DeviceType,
                            Name = data.Name,
                            Description = data.Description,
                            Configs = freshAutogenConfigs.StaticConfigs
                        }
                    };
                cfgService.SetConfigs(basicConfigs);

                cfgService.DKS.SetWithoutDuplicates(freshAutogenConfigs.DynamicKeys, data);

                configs = basicConfigs.FirstOrDefault();

                log.Debug("All configs registered!");

            }
            else
            {
                //On first boot perform check if new values were added to static class and add them to db if needed
                //becouse doing this each time its not so performant
                if (IsFirstBoot == true)
                {
                    //var xxx = T.GetMethod("GetConfigs").Invoke(T, new object[] { data });
                    log.Debug("Its first boot, lets also check existance for all needed vars");
                    freshAutogenConfigs = GetConfigs(T, data);

                    //var res = freshAutogenConfigs.StaticConfigs.Except(configs.Configs).ToList();
                    foreach (var statK in freshAutogenConfigs.StaticConfigs)
                    {
                        var isPresent = configs.Configs.Any(x => x.Key.Equals(statK.Key));
                        if (isPresent == true)
                        {
                            continue;
                        }
                        else
                        {
                            log.Debug($"Will add static key: {statK.Key}, value: {statK.Value}");
                            //will add keys to referenced object, so no need return
                            configs.Configs.Add(statK);
                        }
                    }

                    //Save changes to DB
                    cfgService.SetConfigs(new List<JsonDeviceConfigs>() { configs });
                    cfgService.DKS.SetWithoutDuplicates(freshAutogenConfigs.DynamicKeys, data);

                }
            }

        }

        /// <summary>
        /// Dynamically recreates all needed STATIC keys on file
        /// Resulting Configs will be populated by reference
        /// </summary>
        /// <param name="configs"></param>
        public static void CreateNeededVarsInFile(Type T, bool IsFirstBoot, ref JsonDeviceConfigs configs, GetConfigsDTO data, JObject originalFile = null)
        {

            GetConfigsResultDTO freshAutogenConfigs = null;
            if (configs == null || configs.Configs == null)
            {
                log.Debug("No configs found!");
                //var xxx = T.GetMethod("GetConfigs").Invoke(T, new object[] { data });
                freshAutogenConfigs = GetConfigs(T, data);

                //compose basic configuration based on default values
                var basicConfigs = new JsonDeviceConfigs() {
                            Id = data.DeviceId,
                            DeviceType = data.DeviceType,
                            Name = data.Name,
                            Description = data.Description,
                            Configs = freshAutogenConfigs.StaticConfigs
                        };

                new FileConfigsService().SetConfigs(basicConfigs, originalFile);

                configs = basicConfigs;

                log.Debug("All configs registered!");

            }
            else
            {
                //On first boot perform check if new values were added to static class and add them to db if needed
                //becouse doing this each time its not so performant
                if (IsFirstBoot == true)
                {
                    //var xxx = T.GetMethod("GetConfigs").Invoke(T, new object[] { data });
                    log.Debug("Its first boot, lets also check existance for all needed vars");
                    freshAutogenConfigs = GetConfigs(T, data);

                    //var res = freshAutogenConfigs.StaticConfigs.Except(configs.Configs).ToList();
                    foreach (var statK in freshAutogenConfigs.StaticConfigs)
                    {
                        var isPresent = configs.Configs.Any(x => x.Key.Equals(statK.Key));
                        if (isPresent == true)
                        {
                            continue;
                        }
                        else
                        {
                            log.Debug($"Will add static key: {statK.Key}, value: {statK.Value}");
                            //will add keys to referenced object, so no need return
                            configs.Configs.Add(statK);
                        }
                    }

                    //Save changes to DB
                    new FileConfigsService().SetConfigs(configs, originalFile);

                }
            }

        }


    }
}
