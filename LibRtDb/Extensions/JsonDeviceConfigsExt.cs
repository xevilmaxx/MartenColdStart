using LibRtDb.DTO.DeviceConfigs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace LibRtDb.Extensions
{
    public static class JsonDeviceConfigsExt
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        //private static T ConvertTo<T>(this object Value)
        //{
        //    //return same object if its already of correct type
        //    if (Value is T variable) return variable;

        //    try
        //    {
        //        //Handling Nullable types i.e, int?, double?, bool? .. etc
        //        if (Nullable.GetUnderlyingType(typeof(T)) != null)
        //        {
        //            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(Value);
        //        }

        //        return (T)Convert.ChangeType(Value, typeof(T));
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Warn(ex);
        //        return default(T);
        //    }
        //}

        /// <summary>
        /// Method to query Structure easily
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="deviceConfigs">source</param>
        /// <param name="KeyName">Key to search</param>
        /// <param name="DefaultValue">Value to return if nothing found o unsucceded parse</param>
        /// <returns></returns>
        public static T Parse<T>(this JsonDeviceConfigs deviceConfigs, string KeyName, T DefaultValue)
        {
            try
            {

                var result = DefaultValue;

                var keyN = deviceConfigs?.Configs.Where(x => x.Key.Equals(KeyName)).FirstOrDefault();
                if (keyN != null)
                {

                    //return same object if its already of correct type
                    if (keyN.Value is T variable) return variable;


                    //Handling Nullable types i.e, int?, double?, bool? .. etc
                    if (Nullable.GetUnderlyingType(typeof(T)) != null)
                    {
                        //More articulation in this case, as we had some issues with VM
                        var converter = TypeDescriptor.GetConverter(typeof(T));
                        if (converter.CanConvertTo(typeof(T)))
                        {
                            log.Trace($"[{KeyName} : {keyN.Value}] is convertible to: {typeof(T).FullName}");
                            //mainly parse string to correct type, and this one normally is used
                            result = (T)converter.ConvertFrom(keyN.Value);
                        }
                        else
                        {
                            log.Trace($"[{KeyName} : {keyN.Value}] will be simply casted to: {typeof(T).FullName}");
                            //simply perform a basic cast
                            result = (T)keyN.Value;
                        }                        
                    }
                    //Normal Type
                    else
                    {
                        log.Trace($"[{KeyName} : {keyN.Value}] will simply change type to: {typeof(T).FullName}");
                        result = (T)Convert.ChangeType(keyN.Value, typeof(T));
                    }

                    log.Debug(KeyName + " -> " + result);

                }
                else
                {
                    log.Debug(KeyName + " is not existent key, will set to default: " + result);
                }

                return result;

            }
            catch (Exception ex)
            {
                log.Debug("Cant parse: " + KeyName);
                log.Warn(ex);
                return DefaultValue;
            }
        }

    }
}
