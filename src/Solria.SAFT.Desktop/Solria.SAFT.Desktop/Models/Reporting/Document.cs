using System.Collections.Generic;

namespace SolRIA.SAFT.Desktop.Models.Reporting;

public sealed class Document
{
    public string Pk { get; set; }

    public string Total { get; set; }
    public string NetTotal { get; set; }
    public string VatTotal { get; set; }

    public string Number { get; set; }
    public string ATCUD { get; set; }
    public string Date { get; set; }
    public string SystemDate { get; set; }

    public IEnumerable<DocumentLine> Lines { get; set; }
    public IEnumerable<Tax> Taxes { get; set; }
}
