namespace SolRIA.SAFT.Parser.Models;

public class SaftProgress
{
    /// <summary>
    /// Progresso de 0.0 a 100.0
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Descrição do passo atual (ex: "A ler Documentos de Faturação...")
    /// </summary>
    public string CurrentStep { get; set; } = string.Empty;

    /// <summary>
    /// Detalhe complementar (ex: "Lidas 1 200 faturas (45.2 MB / 120.0 MB)")
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    public SaftProgress() { }

    public SaftProgress(double percentage, string step, string detail = "")
    {
        ProgressPercentage = percentage;
        CurrentStep = step;
        Detail = detail;
    }
}
