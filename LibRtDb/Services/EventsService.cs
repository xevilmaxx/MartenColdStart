using Google.Protobuf.Collections;
using LibRtDb.DTO.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Services
{
    public class EventsService
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        #region Read

       
        #endregion

        #region Write
        public bool SaveEvent(long DeviceId, int EventType)
        {
            try
            {

                if (DeviceId < 0 || EventType < 0)
                {
                    log.Debug(string.Format("DeviceId [{0}] or EventType [{1}] is < 0, wont procede further", DeviceId, EventType));
                    return false;
                }

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    log.Debug($"Registereing event: {EventType} for device: {DeviceId}");

                    var tmpEvent = new DTO.Events.Event
                    {
                        Date = DateTime.Now,
                        DeviceId = DeviceId,
                        EventType = EventType
                    };

                    session.Store(tmpEvent);

                    session.SaveChanges();

                }

                return true;

            }
            catch (Exception ex)
            {
                log.Error(ex, "SaveEvent -> ");
                return false;
            }
        }

        #endregion

        #region Utils
        #endregion

        #region Delete
        #endregion

    }
}
