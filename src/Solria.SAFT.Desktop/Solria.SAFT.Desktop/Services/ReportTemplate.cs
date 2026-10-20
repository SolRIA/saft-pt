using System;

namespace SolRIA.SAFT.Desktop.Services;

/// <summary>Stable identifiers; unrelated reports can share an Excel filename.</summary>
public enum ReportTemplate
{
    Clientes,
    Fornecedores,
    Produtos,
    Impostos,
    ErrosValidacao,
    Inventario,
    LancamentosContabilisticos,
    FaturacaoDocumentos,
    FaturacaoImpostos,
    FaturacaoDocumento,
    MercadoriasDocumentos,
    MercadoriasImpostos,
    MercadoriasDocumento,
    ConferenciaDocumentos,
    ConferenciaImpostos,
    ConferenciaDocumento,
    PagamentosDocumentos,
    PagamentosImpostos,
    PagamentosDocumento,
    PagamentosLinhas
}

internal static class ReportTemplates
{
    // Order is the worksheet/dataset order, including empty datasets.
    internal static int[] ColumnCounts(ReportTemplate template) => template switch
    {
        ReportTemplate.Clientes or ReportTemplate.Fornecedores or ReportTemplate.Produtos or ReportTemplate.Impostos => [7],
        ReportTemplate.ErrosValidacao => [3],
        ReportTemplate.Inventario => [6],
        ReportTemplate.LancamentosContabilisticos => [11],
        ReportTemplate.FaturacaoDocumentos => [11, 15],
        ReportTemplate.FaturacaoDocumento => [15],
        ReportTemplate.FaturacaoImpostos or ReportTemplate.MercadoriasImpostos or ReportTemplate.ConferenciaImpostos or ReportTemplate.PagamentosImpostos => [4],
        ReportTemplate.MercadoriasDocumentos or ReportTemplate.MercadoriasDocumento => [14],
        ReportTemplate.ConferenciaDocumentos or ReportTemplate.ConferenciaDocumento => [17],
        ReportTemplate.PagamentosDocumentos or ReportTemplate.PagamentosDocumento => [11],
        ReportTemplate.PagamentosLinhas => [9],
        _ => throw new ArgumentOutOfRangeException(nameof(template), template, "Modelo de relatório desconhecido.")
    };

    internal static string FileName(ReportTemplate template)
    {
        _ = ColumnCounts(template); // Reject undefined enum values before constructing a path.
        return template + ".rdl";
    }
}
