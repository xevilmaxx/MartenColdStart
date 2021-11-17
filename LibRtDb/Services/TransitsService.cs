using GenericAPIProtos;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Services
{
    /// <summary>
    /// May become part of 1 partial class if its simplier
    /// </summary>
    public class TransitsService
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        #region Read

        /// <summary>
        /// Returns list of all Transits in DB
        /// </summary>
        /// <returns></returns>
        public TransitsToSync GetAllTransits()
        {
            try
            {

                log.Debug("GetAllTransits Invoked!");

                var result = new TransitsToSync();

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    var transits = session.Query<Transit>().ToList();
                    //result.Transits = new RepeatedField<Transit>() { transits };
                    result.Transits.AddRange(transits);

                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new TransitsToSync();
            }
        }

        /// <summary>
        /// Get all transits where IsSyncronyzed field is false
        /// </summary>
        /// <returns></returns>
        public TransitsToSync GetTransitsToSync()
        {
            try
            {

                log.Debug("GetAllTransits Invoked!");

                var result = new TransitsToSync();

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    var transits = session.Query<Transit>().Where(t => t.IsSyncronyzed == false);
                    //result.Transits = new RepeatedField<Transit>() { transits };
                    result.Transits.AddRange(transits);

                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new TransitsToSync();
            }
        }

        /// <summary>
        /// Fetch not syncronyzed transits older than passed parameter
        /// </summary>
        /// <param name="olderThan"></param>
        /// <param name="logging"></param>
        /// <returns></returns>
        public TransitsToSync GetOldTransitsToSync(int olderThan = 60, bool logging = true)
        {
            try
            {
                if (logging)
                {
                    log.Debug("GetOldTransitsToSync Invoked!");
                }

                var result = new TransitsToSync();

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    //this is different from timestamp
                    //var now = DateTime.UtcNow.AddSeconds(-olderThan).Ticks;
                    var now = DateTime.UtcNow.AddSeconds(-olderThan).ToTimestamp();
                    var transits = session.Query<Transit>().Where(t =>
                        t.IsSyncronyzed == false
                        &&
                        (
                            t.LastChange.Seconds < now.Seconds
                            &&
                            t.LastChange.Nanos < now.Nanos
                        )
                    ).ToList();
                    //result.Transits = new RepeatedField<Transit>() { transits };
                    result.Transits.AddRange(transits);

                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new TransitsToSync();
            }
        }

        /// <summary>
        /// Searches in DB most similar transit to passed one without considering ID field
        /// </summary>
        /// <param name="transit"></param>
        /// <returns></returns>
        public Transit GetSimilar(Transit transit)
        {
            try
            {

                log.Debug("GetSimilar without session Invoked!");

                using (var session = DbFactory.GetContext().QuerySession())
                {
                    //its safe to use return like that
                    return GetSimilar(session, transit);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Searches in DB most similar transit to passed one without considering ID field
        /// </summary>
        /// <param name="session"></param>
        /// <param name="transit"></param>
        /// <returns></returns>
        public Transit GetSimilar(IQuerySession session, Transit transit)
        {
            try
            {

                log.Debug("GetSimilar Invoked!");

                Transit result = null;

                IEnumerable<Transit> dynamicQuery = session.Query<Transit>();

                bool isSomeComparisonDone = false;
                if (transit.TitleIn != null)
                {
                    log.Debug("Will add TitleIn check");
                    dynamicQuery = dynamicQuery.Where(x =>
                        x.TitleIn != null
                        &&
                        x.TitleIn.TokenCode.Equals(transit.TitleIn.TokenCode)
                        &&
                        x.TitleIn.Plate.Equals(transit.TitleIn.Plate)
                        &&
                        x.TitleIn.GateIp.Equals(transit.TitleIn.GateIp)
                        &&
                        x.TitleIn.DeviceType.Equals(transit.TitleIn.DeviceType)
                        &&
                        (
                            x.TitleIn.Time.Seconds == transit.TitleIn.Time.Seconds
                            &&
                            x.TitleIn.Time.Nanos == transit.TitleIn.Time.Nanos
                        )
                    );
                    isSomeComparisonDone = true;
                }

                if (transit.Payment != null)
                {
                    log.Debug("Will add Payment title check");
                    dynamicQuery = dynamicQuery.Where(x =>
                        x.Payment.GateId == transit.Payment.GateId
                        &&
                        (
                            x.Payment.Time.Seconds == transit.Payment.Time.Seconds
                            &&
                            x.Payment.Time.Nanos == transit.Payment.Time.Nanos
                        )
                    );
                    isSomeComparisonDone = true;
                }

                if (transit.TitleOut != null)
                {
                    log.Debug("Will add TitleOut check");
                    dynamicQuery = dynamicQuery.Where(x =>
                        x.TitleOut != null
                        &&
                        x.TitleOut.TokenCode.Equals(transit.TitleOut.TokenCode)
                        &&
                        x.TitleOut.Plate.Equals(transit.TitleOut.Plate)
                        &&
                        x.TitleOut.GateIp.Equals(transit.TitleOut.GateIp)
                        &&
                        x.TitleOut.DeviceType.Equals(transit.TitleOut.DeviceType)
                        &&
                        (
                            x.TitleOut.Time.Seconds == transit.TitleOut.Time.Seconds
                            &&
                            x.TitleOut.Time.Nanos == transit.TitleOut.Time.Nanos
                        )
                    );
                    isSomeComparisonDone = true;
                }

                //execute query just if needed, otherwise we risk to pick first casual elemnt from DB on wrong usage
                if (isSomeComparisonDone == true)
                {
                    result = dynamicQuery.FirstOrDefault();
                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns Transit with not empty TitleIn and matching token code [LiteGate].
        /// Will avoid simply refused or unsucceded transits
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public Transit GetByTokenCode(string Code)
        {
            try
            {

                //Last 2 conditions are needed to avoid taking into consideration
                //simply refused transits or not succeded

                Transit result = null;
                using (var session = DbFactory.GetContext().QuerySession())
                {
                    result = session.Query<Transit>().Where(x =>
                        x.TitleIn != null
                        &&
                        x.TitleIn.TokenCode.Equals(Code)
                        &&
                        x.IsAcceptedByGate == true
                        &&
                        x.IsTransitSucceeded == true
                    ).FirstOrDefault();
                }
                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns Transit with not empty TitleIn and matching Plate [LiteGate]
        /// Will avoid simply refused or unsucceded transits
        /// </summary>
        /// <param name="Plate"></param>
        /// <returns></returns>
        public Transit GetByPlate(string Plate)
        {
            try
            {

                //Last 2 conditions are needed to avoid taking into consideration
                //simply refused transits or not succeded

                Transit result = null;
                using (var session = DbFactory.GetContext().QuerySession())
                {
                    result = session.Query<Transit>().Where(x =>
                        x.TitleIn != null
                        &&
                        x.TitleIn.Plate.Equals(Plate)
                        &&
                        x.IsAcceptedByGate == true
                        &&
                        x.IsTransitSucceeded == true
                    ).FirstOrDefault();
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

        public bool PayTransit(TransitPayment data)
        {
            try
            {
                log.Debug("PayTransit Invoked!");

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    var appropriateTransit = session.Query<Transit>().Where(
                        x => 
                        x.TitleIn.TokenCode == data.TokenCode
                        &&
                        x.TitleIn.Plate == data.Plate
                        &&
                        x.IsAcceptedByGate == true
                        &&
                        x.IsTransitSucceeded == true
                        );
                    
                    //deprecated cause its payment time
                    //and on gates transit is delete once terminated, so no sence to worry
                    //if(data.Time != null)
                    //{
                    //    log.Debug("Adding also time check");
                    //    appropriateTransit = appropriateTransit.Where(
                    //        x => 
                    //        x.TitleIn.Time.Seconds == data.Time.Seconds
                    //        &&
                    //        x.TitleIn.Time.Nanos == data.Time.Nanos
                    //        );
                    //}

                    var appropriateTransitResult = appropriateTransit.FirstOrDefault();

                    if(appropriateTransitResult != null)
                    {
                        log.Debug($"Will alter transit with ID: {appropriateTransitResult.Id}");
                        appropriateTransitResult.IsExitPermitted = true;
                        appropriateTransitResult.Payment = data;
                        session.Store(appropriateTransitResult);
                        session.SaveChanges();
                        log.Debug("Payment persisted successfully!");
                    }
                    else
                    {
                        log.Debug("No appropriate transit found to be payed!");
                    }

                }
                return true;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Add or Update
        /// </summary>
        /// <param name="NewTransit"></param>
        /// <returns></returns>
        public bool UpdateTransit(Transit NewTransit)
        {
            try
            {
                log.Debug("UpdateTransit Invoked!");

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    session.Store(NewTransit);
                    session.SaveChanges();

                    //var trs = session.Query<Transit>().ToList();

                }
                return true;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Should work also ad ADD if you pass ID to 0
        /// </summary>
        /// <param name="NewTransits"></param>
        /// <returns></returns>
        public bool UpdateTransit(TransitsToSync NewTransits)
        {

            log.Debug("UpdateTransits Invoked!");

            using (var session = DbFactory.GetContext().LightweightSession())
            {

                try
                {

                    session.StoreObjects(NewTransits.Transits);

                    session.SaveChanges();

                    return true;

                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return false;
                }

            }
        }

        /// <summary>
        /// Finds out if passed transits need to be merged with existent ones or just inserted
        /// </summary>
        /// <param name="NewTransits"></param>
        /// <returns></returns>
        public bool MergeOrUpsertTransit(TransitsToSync NewTransits)
        {

            log.Debug("MergeOrInsertTransit Invoked!");

            using (var session = DbFactory.GetContext().LightweightSession())
            {

                try
                {

                    foreach (var transit in NewTransits.Transits)
                    {

                        var similarInDb = GetSimilar(session, transit);
                        if (similarInDb == null)
                        {
                            log.Debug("No similars found, just add");
                        }
                        else
                        {
                            log.Debug("Merging is needed");
                            //this will update existing db record with new one, due to fact we assigned same ID
                            transit.Id = similarInDb.Id;

                            //Here we mainly need merge 3 major kings of information
                            //Rest of infos for now is not important
                            if (transit.TitleIn == null)
                            {
                                transit.TitleIn = similarInDb.TitleIn;
                            }
                            if (transit.Payment == null)
                            {
                                transit.Payment = similarInDb.Payment;
                            }
                            if (transit.TitleOut == null)
                            {
                                transit.TitleOut = similarInDb.TitleOut;
                            }
                        }

                        session.Store(transit);

                    }

                    session.SaveChanges();

                    return true;

                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return false;
                }

            }
        }

        /// <summary>
        /// Massive Transit insertion, fast and dont want duplicates
        /// </summary>
        /// <param name="NewTransits"></param>
        /// <returns></returns>
        public bool BulkInsertTransit(TransitsToSync NewTransits)
        {
            try
            {

                log.Debug("BulkInsertTransit Invoked!");

                DbFactory.Context.BulkInsert<Transit>(NewTransits.Transits, Marten.BulkInsertMode.InsertsOnly, 100);

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

        /// <summary>
        /// Removes Transit by ID
        /// </summary>
        /// <param name="NewTransit"></param>
        /// <returns></returns>
        public bool RemoveTransit(Transit NewTransit)
        {
            try
            {

                log.Debug("RemoveTransit Invoked!");

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    session.Delete(NewTransit);

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

        /// <summary>
        /// Removes Transits by ID
        /// </summary>
        /// <param name="NewTransits"></param>
        /// <returns></returns>
        public bool RemoveTransit(TransitsToSync NewTransits)
        {

            log.Debug("RemoveTransits Invoked!");

            using (var session = DbFactory.GetContext().LightweightSession())
            {

                try
                {

                    foreach (var tr in NewTransits.Transits)
                    {
                        session.Delete(tr);
                    }

                    session.SaveChanges();

                    return true;

                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return false;
                }

            }
        }

        /// <summary>
        /// Drops all Documents of type => Transit
        /// </summary>
        public void TruncateAllTransit()
        {
            try
            {

                log.Debug("TruncateAllTransit Invoked!");

                if (DbFactory.Context == null)
                {
                    log.Debug("Populating Context");
                    DbFactory.GetContext();
                }

                DbFactory.Context.Advanced.Clean.DeleteDocumentsByType(typeof(Transit));

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public bool FindTransitsToCloseAndRemove(TransitsToSync Transits)
        {
            try
            {

                log.Debug("FindTransitsToRemove Invoked!");

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    //This method is invoked mainly only by out gate Pull call
                    //so we have 100% out title and if there is some similar entry we need delete it
                    //hoping its the same user
                    foreach (var transit in Transits.Transits)
                    {

                        var somethingIsThere = session.Query<Transit>().Where(x =>
                            x.TitleIn.TokenCode == transit.TitleOut.TokenCode
                            &&
                            x.TitleIn.Plate == transit.TitleOut.Plate
                        ).FirstOrDefault();

                        if (somethingIsThere != null)
                        {
                            //delete
                            session.Delete(somethingIsThere);
                        }

                    }

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

    }
}
