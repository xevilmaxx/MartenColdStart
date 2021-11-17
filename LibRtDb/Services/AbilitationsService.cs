using GenericAPIProtos;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Services
{
    public class AbilitationsService
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        #region Read

        public UsersToSync GetAllAbilitations()
        {
            try
            {

                log.Debug("GetAllAbilitations Invoked!");

                var result = new UsersToSync();

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    var xx = session.Query<User>().ToList();
                    //var yy = new RepeatedField<User>() { xx };
                    result.Users.AddRange(xx);

                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new UsersToSync();
            }
        }

        /// <summary>
        /// Get all Abilitation where IsSyncronyzed field is false
        /// </summary>
        /// <returns></returns>
        public UsersToSync GetUsersToSync(bool logging = true)
        {
            try
            {
                if (logging)
                {
                    log.Debug("GetAllAbilitations Invoked!");
                }
                var result = new UsersToSync();

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    var xx = session.Query<User>().Where(u => u.IsSyncronyzed == false);
                    //var yy = new RepeatedField<User>() { xx };
                    result.Users.AddRange(xx);

                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new UsersToSync();
            }
        }
        public User GetUserByToken(string Code)
        {
            try
            {

                User result = null;

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    result = session.Query<User>().Where(u => u.Title.TokenCode.Equals(Code)).FirstOrDefault();

                }

                return result;

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }
        public User GetUserByPlate(string Plate)
        {
            try
            {

                User result = null;

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    result = session.Query<User>().Where(u => u.Title.Plate.Equals(Plate)).FirstOrDefault();

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

        /// <summary>
        /// Add or Update
        /// </summary>
        /// <param name="NewAbilitation"></param>
        /// <returns></returns>
        public bool UpdateAbilitation(User NewAbilitation)
        {
            try
            {

                log.Debug("UpdateAbilitation Invoked!");

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    session.Store(NewAbilitation);

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
        /// Should work also ad ADD if you pass ID to 0
        /// </summary>
        /// <param name="NewTransits"></param>
        /// <returns></returns>
        public bool UpdateAbilitation(UsersToSync NewAbilitations)
        {

            log.Debug("UpdateAbilitations Invoked!");

            using (var session = DbFactory.GetContext().LightweightSession())
            {

                try
                {

                    session.StoreObjects(NewAbilitations.Users);

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

        public bool BulkInsertAbilitation(UsersToSync NewAbilitations)
        {
            try
            {

                log.Debug("BulkInsertAbilitation Invoked!");

                DbFactory.Context.BulkInsert<User>(NewAbilitations.Users, Marten.BulkInsertMode.InsertsOnly, 100);

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

        public bool RemoveAbilitation(User NewAbilitation)
        {
            try
            {

                log.Debug("RemoveAbilitation Invoked!");

                using (var session = DbFactory.GetContext().LightweightSession())
                {

                    session.Delete(NewAbilitation);

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

        public bool RemoveAbilitation(UsersToSync NewAbilitations)
        {

            log.Debug("RemoveAbilitations Invoked!");

            using (var session = DbFactory.GetContext().LightweightSession())
            {

                try
                {

                    foreach (var usr in NewAbilitations.Users)
                    {
                        session.Delete(usr);
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
        /// Remove all records from User collection in DB
        /// </summary>
        public void TruncateAllAbilitation()
        {
            try
            {

                log.Debug("TruncateAllAbilitation Invoked!");

                if (DbFactory.Context == null)
                {
                    log.Debug("Populating Context");
                    DbFactory.GetContext();
                }

                DbFactory.Context.Advanced.Clean.DeleteDocumentsByType(typeof(User));

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        #endregion

    }
}
