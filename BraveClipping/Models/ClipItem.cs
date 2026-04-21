namespace BraveClipping.Models;

public class ClipItem
{
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string UploadUrl { get; set; } = string.Empty;

    public string FileName => Path.GetFileName(FilePath);
}
