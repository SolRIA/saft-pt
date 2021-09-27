using Solria.SAFT.Parser.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.Services
{
    public interface ISaftValidator
    {
        string SaftFileName { get; set; }
        string StockFileName { get; set; }

        string PublicKeyFileName { get; set; }
        string PrivateKeyFileName { get; set; }

        AuditFile SaftFile { get; set; }

        StockFile StockFile { get; set; }

        List<ValidationError> MensagensErro { get; set; }

        int SaftHashValidationNumber { get; set; }
        int SaftHashValidationErrorNumber { get; set; }

        Task OpenSaftFile(string filename);
        Task OpenStockFile(string filename);


    }
}
