using LibRtDb.DTO.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Services
{
    public class LanguageResources
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        #region Read
        public Dictionary<string, string> GetLanguageResources(string Language)
        {
            try
            {

                Dictionary<string, string> result = null;

                using (var session = DbFactory.GetContext().QuerySession())
                {

                    log.Debug("GetLanguageResources for language: " + Language);

                    var lang = session.Query<LanguageResource>().Where(x => x.Language.Equals(Language)).FirstOrDefault();

                    if (lang == null)
                    {
                        log.Debug("No appropriate language found in DB");
                        return null;
                    }

                    var langKeys = lang.Resources.ToDictionary(t => t.Key, t => t.Value);

                    if (langKeys != null)
                    {
                        log.Debug("Collected some keys");
                        result = langKeys;
                    }
                    else
                    {
                        log.Debug("No keys found for that language");
                    }

                }

                return result;

            }
            catch (Exception ex)
            {
                log.Debug(ex, "GetLanguageResources -> ");
                return null;
            }
        }

        #endregion

        #region Write
        #endregion

        #region Utils
        #endregion

        #region Delete
        #endregion

    }
}
