using System.Diagnostics;
using BraveClipping.Models;

namespace BraveClipping.Services;

public class TaskExecutionService
{
    public async Task<string> ExecuteAsync(AppTask task)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {task.Command}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = string.IsNullOrWhiteSpace(task.WorkingDirectory)
                ? Environment.CurrentDirectory
                : task.WorkingDirectory
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return $"[{DateTime.Now:HH:mm:ss}] {task.Name}\n{stdout}\n{stderr}\nExit Code: {process.ExitCode}";
    }
}
