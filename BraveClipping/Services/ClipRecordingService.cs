using ScreenRecorderLib;

public class ClipRecordingService
{
    private Recorder _recorder;

    public void StartRecording(string filePath)
    {
        var options = new RecorderOptions();

        _recorder = Recorder.CreateRecorder(options);
        _recorder.Record(filePath);
    }

    public void StopRecording()
    {
        _recorder?.Stop();
    }
}
