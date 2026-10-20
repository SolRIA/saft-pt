namespace Majorsilence.Reporting.UI.RdlAvalonia.Viewer
{
    public enum ZoomMode
    {
        FitPage,
        FitWidth,
        ActualSize
    }

    internal static class ZoomModeExtensions
    {
        public static string ToDisplayString(this ZoomMode mode) => mode switch
        {
            ZoomMode.FitPage    => "Fit Page",
            ZoomMode.FitWidth   => "Fit Width",
            ZoomMode.ActualSize => "Actual Size",
            _                   => mode.ToString()
        };
    }
}

