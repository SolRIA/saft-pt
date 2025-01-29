using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SolRIA.SAFT.Parser.Models.AT;

public class Root
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("messages")]
    public Messages Messages { get; set; }

    [JsonPropertyName("dataProcessamento")]
    public string DataProcessamento { get; set; }

    [JsonPropertyName("linhas")]
    public List<Linha> Linhas { get; set; }

    [JsonPropertyName("numElementos")]
    public int NumElementos { get; set; }

    [JsonPropertyName("totalElementos")]
    public int TotalElementos { get; set; }
}


