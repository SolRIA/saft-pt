using System.Linq;

namespace Solria.SAFT.Desktop.SQL
{
    /// <summary>
    /// This class manages the db update scripts for the diferent app versions
    /// </summary>
    public class UpdateScripts
    {
        /// <summary>
        /// The current app version for the database
        /// </summary>
        public static int Version = 1;

        public static UpdateScript[] GetUpdateScripts(int version)
        {
            return BuildUpdateScripts().Where(s => s.Version > version).OrderBy(s => s.Order).ToArray();
        }

        private static UpdateScript[] BuildUpdateScripts()
        {
            return new UpdateScript[]
            {
                //new UpdateScript { Version = 1, Order = 1, Sql = ";" }
            };
        }
    }
}
