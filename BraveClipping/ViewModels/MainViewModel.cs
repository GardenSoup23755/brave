using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BraveClipping.Models;
using BraveClipping.Services;

namespace BraveClipping.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly JsonStorageService _storage = new();
    private readonly TaskExecutionService _taskExecution = new();
    private readonly ClipRecordingService _clipRecording = new();
    private readonly CloudinaryUploadService _uploadService = new();

    private readonly string _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveClipping");

    private string _selectedPage = "Dashboard";
    private string _taskName = string.Empty;
    private string _taskCommand = string.Empty;
    private string _taskWorkingDirectory = string.Empty;
    private AppTask? _selectedTask;
    private bool _isRecording;

    public ObservableCollection<AppTask> Tasks { get; } = [];
    public ObservableCollection<ClipItem> Clips { get; } = [];
    public ObservableCollection<string> Logs { get; } = [];

    public SettingsModel Settings { get; private set; } = new();

    public string SelectedPage
    {
        get => _selectedPage;
        set
        {
            _selectedPage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDashboard));
            OnPropertyChanged(nameof(IsTaskManager));
            OnPropertyChanged(nameof(IsClipManager));
            OnPropertyChanged(nameof(IsLogs));
            OnPropertyChanged(nameof(IsSettings));
        }
    }

    public string TaskName
    {
        get => _taskName;
        set { _taskName = value; OnPropertyChanged(); }
    }

    public string TaskCommand
    {
        get => _taskCommand;
        set { _taskCommand = value; OnPropertyChanged(); }
    }

    public string TaskWorkingDirectory
    {
        get => _taskWorkingDirectory;
        set { _taskWorkingDirectory = value; OnPropertyChanged(); }
    }

    public AppTask? SelectedTask
    {
        get => _selectedTask;
        set
        {
            _selectedTask = value;
            OnPropertyChanged();

            if (value != null)
            {
                TaskName = value.Name;
                TaskCommand = value.Command;
                TaskWorkingDirectory = value.WorkingDirectory;
            }
        }
    }

    public bool IsRecording
    {
        get => _isRecording;
        set { _isRecording = value; OnPropertyChanged(); }
    }

    public bool IsDashboard => SelectedPage == "Dashboard";
    public bool IsTaskManager => SelectedPage == "Task Manager";
    public bool IsClipManager => SelectedPage == "Clip Manager";
    public bool IsLogs => SelectedPage == "Output/Logs";
    public bool IsSettings => SelectedPage == "Settings";

    public ICommand NavigateCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand SaveTaskEditsCommand { get; }
    public ICommand RemoveTaskCommand { get; }
    public ICommand RunTaskCommand { get; }
    public ICommand StartRecordingCommand { get; }
    public ICommand StopRecordingCommand { get; }
    public ICommand UploadClipCommand { get; }
    public ICommand CopyLinkCommand { get; }
    public ICommand SaveSettingsCommand { get; }

    public MainViewModel()
    {
        NavigateCommand = new RelayCommand<string>(p => SelectedPage = p ?? "Dashboard");
        AddTaskCommand = new RelayCommand(AddTask);
        SaveTaskEditsCommand = new RelayCommand(SaveTaskEdits);
        RemoveTaskCommand = new RelayCommand<AppTask>(RemoveTask);
        RunTaskCommand = new RelayCommand<AppTask>(async task => await RunTask(task));
        StartRecordingCommand = new RelayCommand(StartRecording, () => !IsRecording);
        StopRecordingCommand = new RelayCommand(StopRecording, () => IsRecording);
        UploadClipCommand = new RelayCommand<ClipItem>(async clip => await UploadClip(clip));
        CopyLinkCommand = new RelayCommand<ClipItem>(clip =>
        {
            if (!string.IsNullOrWhiteSpace(clip?.UploadUrl))
            {
                Clipboard.SetText(clip.UploadUrl);
                AddLog("Copied upload link.");
            }
        });
        SaveSettingsCommand = new RelayCommand(SaveSettings);

        _clipRecording.RecordingCompleted += OnRecordingCompleted;
        _clipRecording.RecordingFailed += err => AddLog($"Recording failed: {err}");

        LoadState();
    }

    private string TasksPath => Path.Combine(_basePath, "tasks.json");
    private string ClipsPath => Path.Combine(_basePath, "clips.json");
    private string SettingsPath => Path.Combine(_basePath, "settings.json");
    private string ClipFolder => Path.Combine(_basePath, "clips");

    private void AddTask()
    {
        if (string.IsNullOrWhiteSpace(TaskName) || string.IsNullOrWhiteSpace(TaskCommand))
        {
            return;
        }

        Tasks.Add(new AppTask
        {
            Name = TaskName,
            Command = TaskCommand,
            WorkingDirectory = TaskWorkingDirectory
        });

        ClearTaskEditor();
        SaveTasks();
        AddLog("Task added.");
    }

    private void SaveTaskEdits()
    {
        if (SelectedTask == null)
        {
            return;
        }

        SelectedTask.Name = TaskName;
        SelectedTask.Command = TaskCommand;
        SelectedTask.WorkingDirectory = TaskWorkingDirectory;

        SaveTasks();
        OnPropertyChanged(nameof(Tasks));
        AddLog($"Task updated: {SelectedTask.Name}");
    }

    private void RemoveTask(AppTask? task)
    {
        if (task == null)
        {
            return;
        }

        Tasks.Remove(task);
        SaveTasks();
        AddLog($"Task removed: {task.Name}");
    }

    private async Task RunTask(AppTask? task)
    {
        if (task == null)
        {
            return;
        }

        try
        {
            var output = await _taskExecution.ExecuteAsync(task);
            AddLog(output);
        }
        catch (Exception ex)
        {
            AddLog($"Task failed: {ex.Message}");
        }
    }

    private void StartRecording()
    {
        Directory.CreateDirectory(ClipFolder);
        var filePath = Path.Combine(ClipFolder, $"clip-{DateTime.Now:yyyyMMdd-HHmmss}.mp4");
        _clipRecording.StartRecording(filePath);
        IsRecording = true;
        AddLog("Recording started.");
        RefreshCommands();
    }

    private void StopRecording()
    {
        _clipRecording.StopRecording();
        IsRecording = false;
        AddLog("Recording stopping...");
        RefreshCommands();
    }

    private async Task UploadClip(ClipItem? clip)
    {
        if (clip == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Settings.CloudName) || string.IsNullOrWhiteSpace(Settings.ApiKey) || string.IsNullOrWhiteSpace(Settings.ApiSecret))
        {
            AddLog("Set Cloudinary credentials in Settings first.");
            return;
        }

        try
        {
            clip.UploadUrl = await _uploadService.UploadAsync(Settings, clip.FilePath);
            SaveClips();
            OnPropertyChanged(nameof(Clips));
            AddLog($"Uploaded clip: {clip.FileName}");
        }
        catch (Exception ex)
        {
            AddLog($"Upload failed: {ex.Message}");
        }
    }

    private void SaveSettings()
    {
        _storage.Save(SettingsPath, Settings);
        AddLog("Settings saved.");
    }

    private void SaveTasks() => _storage.Save(TasksPath, Tasks.ToList());

    private void SaveClips() => _storage.Save(ClipsPath, Clips.ToList());

    private void LoadState()
    {
        Directory.CreateDirectory(_basePath);

        foreach (var t in _storage.Load(TasksPath, new List<AppTask>()))
        {
            Tasks.Add(t);
        }

        foreach (var clip in _storage.Load(ClipsPath, new List<ClipItem>()))
        {
            Clips.Add(clip);
        }

        Settings = _storage.Load(SettingsPath, new SettingsModel());
        OnPropertyChanged(nameof(Settings));
    }

    private void OnRecordingCompleted(string filePath)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Clips.Insert(0, new ClipItem
            {
                FilePath = filePath,
                CreatedAt = DateTime.Now
            });
            SaveClips();
            AddLog($"Recording saved: {Path.GetFileName(filePath)}");
        });
    }

    private void AddLog(string message)
    {
        Logs.Insert(0, message);
    }

    private void ClearTaskEditor()
    {
        TaskName = string.Empty;
        TaskCommand = string.Empty;
        TaskWorkingDirectory = string.Empty;
    }

    private void RefreshCommands()
    {
        (StartRecordingCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (StopRecordingCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
}
