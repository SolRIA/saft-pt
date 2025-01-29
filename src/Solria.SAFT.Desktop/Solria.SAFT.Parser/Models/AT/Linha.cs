using System.Text.Json.Serialization;

namespace SolRIA.SAFT.Parser.Models.AT;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class Linha
{
    [JsonPropertyName("idDocumento")]
    public object IdDocumento { get; set; }

    [JsonPropertyName("origemRegisto")]
    public string OrigemRegisto { get; set; }

    [JsonPropertyName("origemRegistoDesc")]
    public string OrigemRegistoDesc { get; set; }

    [JsonPropertyName("nifEmitente")]
    public int NifEmitente { get; set; }

    [JsonPropertyName("nomeEmitente")]
    public object NomeEmitente { get; set; }

    [JsonPropertyName("nifAdquirente")]
    public int NifAdquirente { get; set; }

    [JsonPropertyName("nomeAdquirente")]
    public object NomeAdquirente { get; set; }

    [JsonPropertyName("paisAdquirente")]
    public string PaisAdquirente { get; set; }

    [JsonPropertyName("nifAdquirenteInternac")]
    public object NifAdquirenteInternac { get; set; }

    [JsonPropertyName("tipoDocumento")]
    public string TipoDocumento { get; set; }

    [JsonPropertyName("tipoDocumentoDesc")]
    public string TipoDocumentoDesc { get; set; }

    [JsonPropertyName("numerodocumento")]
    public string NumeroDocumento { get; set; }

    [JsonPropertyName("hashDocumento")]
    public object HashDocumento { get; set; }

    [JsonPropertyName("dataEmissaoDocumento")]
    public string DataEmissaoDocumento { get; set; }

    [JsonPropertyName("valorTotal")]
    public int ValorTotal { get; set; }

    [JsonPropertyName("valorTotalBaseTributavel")]
    public int ValorTotalBaseTributavel { get; set; }

    [JsonPropertyName("valorTotalIva")]
    public int ValorTotalIva { get; set; }

    [JsonPropertyName("valorTotalBeneficioProv")]
    public object ValorTotalBeneficioProv { get; set; }

    [JsonPropertyName("valorTotalSetorBeneficio")]
    public object ValorTotalSetorBeneficio { get; set; }

    [JsonPropertyName("valorTotalDespesasGerais")]
    public object ValorTotalDespesasGerais { get; set; }

    [JsonPropertyName("estadoBeneficio")]
    public string EstadoBeneficio { get; set; }

    [JsonPropertyName("estadoBeneficioDesc")]
    public string EstadoBeneficioDesc { get; set; }

    [JsonPropertyName("estadoBeneficioEmitente")]
    public string EstadoBeneficioEmitente { get; set; }

    [JsonPropertyName("estadoBeneficioDescEmitente")]
    public string EstadoBeneficioDescEmitente { get; set; }

    [JsonPropertyName("existeTaxaNormal")]
    public object ExisteTaxaNormal { get; set; }

    [JsonPropertyName("actividadeEmitente")]
    public string ActividadeEmitente { get; set; }

    [JsonPropertyName("actividadeEmitenteDesc")]
    public string ActividadeEmitenteDesc { get; set; }

    [JsonPropertyName("actividadeProf")]
    public object ActividadeProf { get; set; }

    [JsonPropertyName("actividadeProfDesc")]
    public object ActividadeProfDesc { get; set; }

    [JsonPropertyName("comunicacaoComerciante")]
    public bool ComunicacaoComerciante { get; set; }

    [JsonPropertyName("comunicacaoConsumidor")]
    public bool ComunicacaoConsumidor { get; set; }

    [JsonPropertyName("isDocumentoEstrangeiro")]
    public bool IsDocumentoEstrangeiro { get; set; }

    [JsonPropertyName("atcud")]
    public string ATCUD { get; set; }

    [JsonPropertyName("autofaturacao")]
    public bool AutoFaturacao { get; set; }

    public double Total { get; set; }
}


