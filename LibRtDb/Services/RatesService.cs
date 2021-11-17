using GenericAPIProtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Services
{
    public class RatesService
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        #region Read

        public Rate GetLocalRate()
        {
            try
            {

                log.Debug("GetLocalRate Invoked!");

                var result = new Rate();

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    result = session.Query<Rate>().FirstOrDefault();

                }

                if(result.WeekModel == null)
                {
                    log.Warn("No Rate Were Retrieved!");
                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        #endregion

        #region Write

        public bool SetLocalRate(Rate Data)
        {
            try
            {

                log.Debug("SetLocalRate Invoked!");

                //TruncateAllRates();

                using (var session = DbFactory.GetContext().LightweightSession())
                {
                    session.Store<Rate>(Data);
                    session.SaveChanges();
                }

                return true;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        #endregion

        #region Utils
        #endregion

        #region Delete

        public bool TruncateAllRates()
        {
            try
            {

                log.Debug("TruncateAllRates Invoked!");

                if (DbFactory.Context == null)
                {
                    log.Debug("Populating Context");
                    DbFactory.GetContext();
                }

                DbFactory.Context.Advanced.Clean.DeleteDocumentsByType(typeof(Rate));

                return true;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        #endregion

    }
}
