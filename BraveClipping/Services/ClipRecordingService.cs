using ScreenRecorderLib;

namespace BraveClipping.Services;

public class ClipRecordingService
{
    private Recorder? _recorder;

    public event Action<string>? RecordingCompleted;
    public event Action<string>? RecordingFailed;

    public void StartRecording(string outputPath)
    {
        var options = new RecorderOptions
        {
            RecorderMode = RecorderMode.Video,
            IsThrottlingDisabled = false,
            IsHardwareEncodingEnabled = true,
            IsLowLatencyEnabled = false,
            VideoOptions = new VideoOptions
            {
                BitrateMode = BitrateControlMode.Quality,
                Framerate = 30,
                IsFixedFramerate = true,
                EncoderProfile = H264Profile.Main,
                Quality = 80
            },
            AudioOptions = new AudioOptions
            {
                IsAudioEnabled = false
            }
        };

        _recorder = Recorder.CreateRecorder(options);
        _recorder.OnRecordingComplete += (_, e) => RecordingCompleted?.Invoke(e.FilePath);
        _recorder.OnRecordingFailed += (_, e) => RecordingFailed?.Invoke(e.Error);
        _recorder.Record(outputPath);
    }

    public void StopRecording()
    {
        _recorder?.Stop();
    }
}
