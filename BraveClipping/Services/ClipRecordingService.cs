using ScreenRecorderLib;
using System;

public class ClipRecordingService
{
    private Recorder? _recorder;

    public event Action<string>? RecordingCompleted;
    public event Action<string>? RecordingFailed;

    public void StartRecording(string filePath)
    {
        var options = new RecorderOptions();

        _recorder = Recorder.CreateRecorder(options);

        _recorder.OnRecordingComplete += (s, e) =>
        {
            RecordingCompleted?.Invoke(filePath);
        };

        _recorder.OnRecordingFailed += (s, e) =>
        {
            RecordingFailed?.Invoke(e.Error);
        };

        _recorder.Record(filePath);
    }

    public void StopRecording()
    {
        _recorder?.Stop();
    }
}
