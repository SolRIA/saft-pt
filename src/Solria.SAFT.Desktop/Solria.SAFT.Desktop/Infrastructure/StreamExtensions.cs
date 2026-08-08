using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.Infrastructure;

public static class StreamExtensions
{
    public static async Task Save(this Stream stream, byte[] data)
    {
        if (stream == null) return;

        await stream.WriteAsync(data).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);

        stream.Close();
        await stream.DisposeAsync().ConfigureAwait(false);
    }

    public static async Task Save(this Stream stream, string data)
    {
        if (stream == null) return;

        await stream.WriteAsync(Encoding.UTF8.GetBytes(data)).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);

        stream.Close();
        await stream.DisposeAsync().ConfigureAwait(false);
    }

    public static async Task<byte[]> ConvertToArray(this Stream stream)
    {
        if (stream == null) return [];

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
        return memoryStream.ToArray();
    }

    public static async Task<string[]> ConvertToStringArray(this Stream stream)
    {
        if (stream == null) return [];

        // Read the stream as a string
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);

        // Split the content by new lines and return as an array
        return content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
    }
}
