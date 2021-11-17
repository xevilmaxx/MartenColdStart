using AutoMapper;
using GenericAPIProtos;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Mappers
{
    public class CustomMapper
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private MapperConfiguration Config { get; set; } = null;

        public Mapper Map { get; private set; } = null;

        public CustomMapper()
        {
            try
            {
                Config = GenerateConfiguration();
                Map = new Mapper(Config);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private MapperConfiguration GenerateConfiguration()
        {

            var config = new MapperConfiguration(cfg =>
            {
                //Mapping(ref cfg);
            });

            return config;

        }

        //private void Mapping(ref IMapperConfigurationExpression cfg)
        //{
        //    DateTime tempDate = DateTime.Now;
        //    int tempInt = 0;

        //    cfg.CreateMap<LibRtDb.Tables.Transits, GenericAPIProtos.Transit>()
        //        .ForMember(dest => dest.Id, act => act.MapFrom(src => src.Id ))
        //        .ForMember(dest => dest.LastChange, act => act.MapFrom(src => src.LastChange.ToTimestamp()))
        //        .ForMember(dest => dest.GateId, act => act.MapFrom(src => src.GateId))
        //        .ForMember(dest => dest.EntryTime, act => act.MapFrom(src => src.EntryTime.ToTimestamp()))
        //        .ForMember(dest => dest.PaymentTime, act => act.MapFrom(src => src.PaymentTime.GetValueOrDefault().ToTimestamp()))
        //        .ForMember(dest => dest.Plate, act => act.MapFrom(src => src.Plate ?? ""))
        //        .ForMember(dest => dest.TokenCode, act => act.MapFrom(src => src.TokenCode ?? ""))
        //        .ForMember(dest => dest.ReaderType, act => act.MapFrom(src => src.ReaderType))
        //        .ForMember(dest => dest.IsExitPermitted, act => act.MapFrom(src => src.IsExitPermitted))
        //        .ForMember(dest => dest.Reason, act => act.MapFrom(src => src.Reason ?? ""))
        //        .ForMember(dest => dest.IsSyncronyzed, act => act.MapFrom(src => src.IsSyncronyzed))
        //        .ReverseMap()
        //        //.ForMember(dest => dest.Last_Change, act => act.Condition(src => (!string.IsNullOrEmpty(src.UltimaModifica))))
        //        //.ForMember(dest => dest.Entry_Time, act => act.Condition(src => (!string.IsNullOrEmpty(src.OraIngresso))))
        //        //.ForMember(dest => dest.Payment_Time, act => act.Condition(src => (!string.IsNullOrEmpty(src.DataPagamento))))
        //        //.ForMember(dest => dest.Plate, act => act.Condition(src => (!string.IsNullOrEmpty(src.Targa))))
        //        //.ForMember(dest => dest.Token_Code, act => act.Condition(src => (!string.IsNullOrEmpty(src.CodiceToken))))
        //        //.ForMember(dest => dest.Reader_Type, act => act.Condition(src => (!string.IsNullOrEmpty(src.TipoLettore))))
        //        //.ForMember(dest => dest.Is_Exit_Permitted, act => act.Condition(src => (!string.IsNullOrEmpty(src.UscitaConsentita))))
        //        //.ForMember(dest => dest.Reason, act => act.Condition(src => (!string.IsNullOrEmpty(src.Motivo))))
        //        .ForMember(dest => dest.LastChange, act => act.MapFrom(src => src.LastChange.ToDateTime()))
        //        .ForMember(dest => dest.EntryTime, act => act.MapFrom(src => src.EntryTime.ToDateTime()))
        //        .ForMember(dest => dest.PaymentTime, act => act.MapFrom(src => src.PaymentTime.ToDateTime()))
        //        .ForMember(dest => dest.Plate, act => act.Condition(src => (!string.IsNullOrEmpty(src.Plate))))
        //        .ForMember(dest => dest.TokenCode, act => act.MapFrom(src => !string.IsNullOrEmpty(src.TokenCode) ? src.TokenCode : " "))
        //        .ForMember(dest => dest.ReaderType, act => act.MapFrom(src => src.ReaderType))
        //        .ForMember(dest => dest.IsExitPermitted, act => act.MapFrom(src => src.IsExitPermitted))
        //        .ForMember(dest => dest.Reason, act => act.Condition(src => (!string.IsNullOrEmpty(src.Reason))))
        //        .ForMember(dest => dest.IsSyncronyzed, act => act.MapFrom(src => src.IsSyncronyzed))
        //        ;


        //    cfg.CreateMap<LibRtDb.Tables.Abilitations, GenericAPIProtos.User>()
        //        .ForMember(dest => dest.Id, act => act.MapFrom(src => src.Id))
        //        .ForMember(dest => dest.TokenCode, act => act.MapFrom(src => src.TokenCode))
        //        .ForMember(dest => dest.Reason, act => act.MapFrom(src => src.Reason))
        //        .ForMember(dest => dest.IsSyncronyzed, act => act.MapFrom(src => src.IsSyncronyzed))
        //        .ForMember(dest => dest.IsAlloved, act => act.MapFrom(src => src.IsAlloved))
        //        .ForMember(dest => dest.ValidFrom, act => act.MapFrom(src => src.ValidFrom.ToTimestamp()))
        //        .ForMember(dest => dest.ValidTo, act => act.MapFrom(src => src.ValidTo.ToTimestamp()))
        //        .ForMember(dest => dest.LastChange, act => act.MapFrom(src => src.LastChange.ToTimestamp()))
        //        .ForMember(dest => dest.ReaderType, act => act.MapFrom(src => src.ReaderType))
        //        .ReverseMap()
        //        .ForMember(dest => dest.TokenCode, act => act.MapFrom(src => src.TokenCode))
        //        .ForMember(dest => dest.IsAlloved, act => act.MapFrom(src => src.IsAlloved))
        //        .ForMember(dest => dest.IsSyncronyzed, act => act.MapFrom(src => src.IsSyncronyzed))
        //        .ForMember(dest => dest.LastChange, act => act.MapFrom(src => src.LastChange.ToDateTime()))
        //        .ForMember(dest => dest.ReaderType, act => act.MapFrom(src => src.ReaderType))
        //        .ForMember(dest => dest.Reason, act => act.MapFrom(src => src.Reason))
        //        .ForMember(dest => dest.ValidFrom, act => act.MapFrom(src => src.ValidFrom.ToDateTime()))
        //        .ForMember(dest => dest.ValidTo, act => act.MapFrom(src => src.ValidTo.ToDateTime()))
        //        ;

        //}

    }
}
