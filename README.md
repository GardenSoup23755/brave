# Brave Clipping

Brave Clipping is a lightweight Windows desktop app (WPF + .NET 8) for running task commands, recording full-screen clips, uploading clips to Cloudinary, and sharing links quickly.

## Download

[![Download Latest Release](https://img.shields.io/badge/Download-Latest%20Release-orange?style=for-the-badge)](https://github.com/USER/REPO/releases/latest)

## Features

- Vice-inspired sidebar workflow and tool pages
- Task Manager with JSON-based task storage
- Command/script execution with output logs
- Clip recording to `.mp4`
- Clip upload to Cloudinary with secure URL
- One-click link copy
- Settings page for Cloudinary credentials
- Custom orange/black/white Brave Clipping in-app vector logo

## Quick Start

1. Download the latest release from the Releases page.
2. Extract and run `BraveClipping.exe`.
3. Open **Settings**, enter Cloudinary credentials, and save.
4. Go to **Clip Manager** and click **Record**.
5. Click **Stop** to save the clip.
6. Click **Upload** on a clip.
7. Click **Copy Link** and share instantly.

## Build Locally

```bash
dotnet publish BraveClipping/BraveClipping.csproj -c Release -r win-x64 --self-contained true
test
```

## Notes

- Built for Windows x64.
- User data is stored in `%LOCALAPPDATA%\BraveClipping`.
