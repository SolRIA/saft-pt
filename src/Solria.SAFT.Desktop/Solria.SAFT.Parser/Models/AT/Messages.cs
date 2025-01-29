using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SolRIA.SAFT.Parser.Models.AT;

public class Messages
{
    [JsonPropertyName("error")]
    public List<object> Error { get; set; }

    [JsonPropertyName("success")]
    public List<object> Success { get; set; }

    [JsonPropertyName("info")]
    public List<object> Info { get; set; }

    [JsonPropertyName("warning")]
    public List<object> Warning { get; set; }
}


