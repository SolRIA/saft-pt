namespace SolRIA.SAFT.Parser.Models
{
    public class PemFile
    {
        public int Id { get; set; }
        public bool PrivateKey { get; set; }
        public string Name { get; set; }
        public string PemText { get; set; }
        public string RsaSettings { get; set; }
    }
}
