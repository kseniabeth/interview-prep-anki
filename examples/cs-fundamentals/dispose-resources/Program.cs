using System.IO;
using System.Threading;
using System.Threading.Tasks;

static async Task<byte[]> ReadFileAsync(string path, CancellationToken token)
{
    await using var input = File.OpenRead(path);
    using var output = new MemoryStream();
    await input.CopyToAsync(output, token);
    return output.ToArray();
}

string path = Path.GetTempFileName();
try
{
    await File.WriteAllBytesAsync(path, new byte[] { 1, 2, 3 });
    byte[] result = await ReadFileAsync(path, CancellationToken.None);
    if (result.Length != 3 || result[0] != 1 || result[2] != 3)
        throw new System.Exception("Unexpected file bytes");
    using var reopened = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
    System.Console.WriteLine("Read and released");
}
finally
{
    File.Delete(path);
}
