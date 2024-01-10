using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System.Collections.Generic;
using System.Linq;

namespace SolRIA.SAFT.Desktop.ViewModels;

public sealed partial class DialogDocumentReferencesViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IEnumerable<SourceDocumentsWorkingDocumentsWorkDocumentLine> allLines;
    public DialogDocumentReferencesViewModel(Models.Reporting.Document doc)
    {
        WorkDocument = doc;

        saftValidator = AppBootstrap.Resolve<ISaftValidator>();

        PreviousDocuments = new List<Models.Reporting.Document>();
        AfterDocuments = new List<Models.Reporting.Document>();

        allLines = saftValidator.SaftFile?.SourceDocuments?.WorkingDocuments?.WorkDocument?.SelectMany(d => d.Line) ?? [];
    }

    public void InitWorkingDocumentDescendents(string pk)
    {
        if (string.IsNullOrWhiteSpace(pk)) return;

        // origin
        var docNo = allLines
            .Where(l => l.OrderReferences != null && l.OrderReferences.Length != 0)
            .Where(l => l.OrderReferences.Any(o => o.OriginatingON == pk))
            .Select(l => l.DocNo)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(docNo)) return;

        var doc = saftValidator.SaftFile?.SourceDocuments?.WorkingDocuments?.WorkDocument
            .FirstOrDefault(d => string.Equals(d.DocumentNumber, docNo, System.StringComparison.OrdinalIgnoreCase));

        if (doc == null) return;

        AfterDocuments.Add(GetDocument(doc));

        InitWorkingDocumentDescendents(docNo);
    }

    public void InitWorkingDocumentParents(string pk)
    {
        var doc = saftValidator.SaftFile?.SourceDocuments?.WorkingDocuments?.WorkDocument
            .FirstOrDefault(d => string.Equals(d.DocumentNumber, pk, System.StringComparison.OrdinalIgnoreCase));

        if (doc == null) return;

        var parentPk = doc.Line
            .Where(l => l.OrderReferences != null && l.OrderReferences.Length > 0)
            .Select(l => l.OrderReferences[0].OriginatingON)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(parentPk)) return;

        var parent = saftValidator.SaftFile?.SourceDocuments?.WorkingDocuments?.WorkDocument
            .FirstOrDefault(d => string.Equals(d.DocumentNumber, parentPk, System.StringComparison.OrdinalIgnoreCase));

        if (parent == null) return;

        PreviousDocuments.Add(GetDocument(parent));

        InitWorkingDocumentParents(parentPk);
    }

    public void InitWorkingDocumentInvoice()
    {
        if(AfterDocuments.Any() == false) return;

        // get the most recent working document
        var lastDoc = AfterDocuments.Last() ?? WorkDocument;

        var allInvoiceLines = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice?.SelectMany(d => d.Line) ?? [];

        var invoiceNo = allInvoiceLines
            .Where(l => l.OrderReferences != null && l.OrderReferences.Length != 0)
            .Where(l => l.OrderReferences.Any(o => o.OriginatingON == lastDoc.Number))
            .Select(l => l.InvoiceNo)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(invoiceNo)) return;

        var invoice = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice
            .FirstOrDefault(d => string.Equals(d.InvoiceNo, invoiceNo, System.StringComparison.OrdinalIgnoreCase));

        InvoiceDocument = GetDocument(invoice);
    }

    [ObservableProperty]
    private Models.Reporting.Document workDocument;

    [ObservableProperty]
    private Models.Reporting.Document invoiceDocument;

    [ObservableProperty]
    private IList<Models.Reporting.Document> previousDocuments;

    [ObservableProperty]
    private IList<Models.Reporting.Document> afterDocuments;

    private static Models.Reporting.Document GetDocument(SourceDocumentsWorkingDocumentsWorkDocument doc)
    {
        var document = new Models.Reporting.Document
        {
            Pk = doc.Pk,
            Number = doc.DocumentNumber,
            ATCUD = doc.ATCUD,
            Date = doc.WorkDate.ToLongDateString(),
            SystemDate = doc.SystemEntryDate.ToLongDateString(),
            Total = doc.DocumentTotals.GrossTotal.ToString("C"),
            NetTotal = doc.DocumentTotals.NetTotal.ToString("C"),
            VatTotal = doc.DocumentTotals.TaxPayable.ToString("C")
        };

        return document;
    }

    private static Models.Reporting.Document GetDocument(SourceDocumentsSalesInvoicesInvoice doc)
    {
        var document = new Models.Reporting.Document
        {
            Pk = doc.Pk,
            Number = doc.InvoiceNo,
            ATCUD = doc.ATCUD,
            Date = doc.InvoiceDate.ToLongDateString(),
            SystemDate = doc.SystemEntryDate.ToLongDateString(),
            Total = doc.DocumentTotals.GrossTotal.ToString("C"),
            NetTotal = doc.DocumentTotals.NetTotal.ToString("C"),
            VatTotal = doc.DocumentTotals.TaxPayable.ToString("C")
        };

        return document;
    }
}
