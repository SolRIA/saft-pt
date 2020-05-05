using Solria.SAFT.Desktop.Models;
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

        Models.SaftV4.AuditFile SaftFileV4 { get; set; }
        Models.SaftV3.AuditFile SaftFileV3 { get; set; }

        Models.StocksV2.StockFile StockFile { get; set; }

        List<Error> MensagensErro { get; set; }

        int SaftHashValidationNumber { get; set; }
        int SaftHashValidationErrorNumber { get; set; }

        Task OpenSaftFileV4(string filename);
        Task OpenSaftFileV3(string filename);
        Task OpenStockFile(string filename);


    }
}
