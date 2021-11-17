using LibRtDb.DTO.DeviceConfigs;
using LibRtDb.DTO.RuntimeConfigs;
using LibRtDb.Extensions;
using LibRtDb.Services;
using MicroService0.DTO;
using Microsoft.Extensions.Hosting;
using Oakton;
using System;
using System.Linq;
using System.Threading;

namespace MicroService0
{
    internal class Program
    {

        private static ConfigsService cfgService { get; set; } = new ConfigsService();

        static void Main(string[] args)
        {

            _ = Host.CreateDefaultBuilder(args).RunOaktonCommands(args);

            Console.WriteLine("Hello World!");

            LibRtDb.DbFactory.EnsureDbStarted();

            //Populate variables values from DB configurations
            //load variables
            LoadConstants();

            Console.WriteLine(" ");
            Console.WriteLine("###################################################");
            Console.WriteLine("Now more intercommunication stuff will come, but much less DB load");
            Console.WriteLine("###################################################");

            int counter = 0;
            while (true)
            {
                if(counter >= 10)
                {
                    Console.WriteLine("Doing something simultaion");
                    counter = 0;
                    var coreConfigs = cfgService.GetConfigs((long)1);
                }
                
                counter++;
                Thread.Sleep(100);
            }

            Console.ReadLine();

        }

        public static void LoadConstants()
        {
            try
            {

                //get device configs
                var deviceConfigs = cfgService.GetConfigs((long)2);

                //var coreConfigs = cfgService.GetConfigs((long)DeviceType.CorpiCore);

                GenericRuntimeConfigs.CreateNeededVarsInDB(
                    typeof(RunCfgs),
                    true,
                    ref deviceConfigs,
                    new GetConfigsDTO()
                    {
                        DeviceId = 2,
                        DeviceType = 2,
                        Name = "2"
                    });

                RunCfgs.OwnServerPort = deviceConfigs.Parse("listening_port", RunCfgs.OwnServerPort);
                RunCfgs.Addresses = deviceConfigs.Parse(nameof(RunCfgs.Addresses), RunCfgs.Addresses);

                RunCfgs.DisabledInputs = deviceConfigs.Parse(nameof(RunCfgs.DisabledInputs), RunCfgs.DisabledInputs);
                RunCfgs.IsPubliclyVisibleService = deviceConfigs.Parse(nameof(RunCfgs.IsPubliclyVisibleService), RunCfgs.IsPubliclyVisibleService);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
