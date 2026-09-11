using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.Services;

public interface ISaftValidator
{
    string SaftFileName { get; set; }
    string StockFileName { get; set; }

    string PublicKeyFileName { get; set; }
    string PrivateKeyFileName { get; set; }

    AuditFile SaftFile { get; set; }

    StockFile StockFile { get; set; }


    int SaftHashValidationNumber { get; set; }
    int SaftHashValidationErrorNumber { get; set; }

    bool UseNewParser { get; set; }

    Task OpenSaftFile(string filename, IProgress<SaftProgress> progress = null);
    Task OpenStockFile(string filename, IProgress<SaftProgress> progress = null);

    IList<ValidationError> GetErrors();
}
