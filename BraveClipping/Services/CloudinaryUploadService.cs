using BraveClipping.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BraveClipping.Services;

public class CloudinaryUploadService
{
    public async Task<string> UploadAsync(SettingsModel settings, string filePath)
    {
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        var cloudinary = new Cloudinary(account);

        await using var stream = File.OpenRead(filePath);
        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(Path.GetFileName(filePath), stream),
            PublicId = $"brave-clipping-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
        };

        var result = await cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
        {
            throw new InvalidOperationException(result.Error.Message);
        }

        return result.SecureUrl?.ToString() ?? string.Empty;
    }
}
