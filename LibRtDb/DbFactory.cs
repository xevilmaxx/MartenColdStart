using GenericAPIProtos;
using LamarCodeGeneration;
using LibRtDb.DTO;
using LibRtDb.DTO.DeviceConfigs;
using LibRtDb.DTO.DynamicKeys;
using LibRtDb.DTO.Events;
using LibRtDb.DTO.Languages;
using LibRtDb.DTO.SerialNumbers;
using LibRtDb.Extensions.Linq.MyEquals;
using Marten;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace LibRtDb
{
    public static class DbFactory
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static DocumentStore Context { get; private set; } = null;

        /// <summary>
        /// Locks thread execution until DB is ready to accept us
        /// </summary>
        public static void EnsureDbStarted()
        {
            try
            {
                log.Debug("EnsureDbStarted Invoked!");

                DocumentStore res = null;
                do
                {
                    res = GetContext();
                    if(res == null)
                    {
                        log.Debug("Will wait a bit before attempting next connection!");
                        Thread.Sleep(5000);
                    }
                } 
                while (res == null);


                log.Debug("EnsureDbStarted DONE!");
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        public static DocumentStore GetContext()
        {
            try
            {

                if(Context == null)
                {

                    //var cfg = GetConfigs();
                    var connectionString = GetConnectionString();

                    log.Debug($"Connection String: {connectionString}");

                    Context = DocumentStore.For(_ => 
                    {

                        //Custom connection string
                        _.Connection(connectionString);

                        _.NameDataLength = 250;

                        //Custom Schema Name
                        _.DatabaseSchemaName = "MartenColdStartTest";

                        _.GeneratedCodeMode = TypeLoadMode.LoadFromPreBuiltAssembly;

                        //add json indexes also
                        // Add a gin index to Company's json data storage
                        //may be commented if performance is poor and you dont need do much readings
                        _.Schema.For<Transit>().GinIndexJsonData();
                        _.Schema.For<User>().GinIndexJsonData();
                        _.Schema.For<JsonDeviceConfigs>().GinIndexJsonData();
                        _.Schema.For<DynamicKey>().GinIndexJsonData();
                        _.Schema.For<Event>().GinIndexJsonData();
                        _.Schema.For<LanguageResource>().GinIndexJsonData();
                        _.Schema.For<SerialNumber>().GinIndexJsonData();

                        //My custom override for comparing objects not supported by Marten natively (for now alpha-8)
                        //Doable only in Marten >= v4
                        //_.Linq.MethodCallParsers.Add(new LinqEqualsExt());

                    });

                }
                return Context;

            }
            catch(Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        private static string GetConnectionString()
        {
            try
            {
                log.Debug("GetConfigs Invoked!");

                string connStr = "host=127.0.0.1;database=postgres;password=pwd;username=postgres;";
                string jsonSettingsFile = "AppSettings.json";

                if (File.Exists(jsonSettingsFile) == true)
                {

                    log.Debug("JSON Config File Found!");

                    JObject data = JObject.Parse(File.ReadAllText(jsonSettingsFile, Encoding.UTF8));

                    connStr = data["ConnectionStrings"]["Postgres"].ToString();

                }
                else if (ConfigurationManager.ConnectionStrings["Postgres"] != null)
                {

                    log.Debug("Will try to search in App.config");
                    
                    connStr = ConfigurationManager.ConnectionStrings["Postgres"].ConnectionString;

                }
                else
                {
                    log.Debug($"Will leave to default value: {connStr}");
                }

                return connStr;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
            
        }

    }
}
