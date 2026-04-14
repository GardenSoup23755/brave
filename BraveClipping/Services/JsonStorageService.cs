using System.Text.Json;

namespace BraveClipping.Services;

public class JsonStorageService
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public T Load<T>(string path, T fallback)
    {
        if (!File.Exists(path))
        {
            return fallback;
        }

        var raw = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(raw, _options) ?? fallback;
    }

    public void Save<T>(string path, T data)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var raw = JsonSerializer.Serialize(data, _options);
        File.WriteAllText(path, raw);
    }
}
