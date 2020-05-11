namespace Solria.SAFT.Desktop.SQL
{
    /// <summary>
    /// Class that hold the data for the db update scripts
    /// </summary>
    public class UpdateScript
    {
        public int Version { get; set; }
        public int Order { get; set; }
        public string Sql { get; set; }
    }
}
