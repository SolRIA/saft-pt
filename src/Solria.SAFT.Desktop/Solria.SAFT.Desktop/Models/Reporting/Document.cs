using System.Collections.Generic;

namespace Solria.SAFT.Desktop.Models.Reporting
{
    public class Document
    {
        public IEnumerable<DocumentLine> Lines { get; set; }
        public IEnumerable<Tax> Taxes { get; set; }
    }
}
