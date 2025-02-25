# BSE Audio Recorder

A lightweight Windows application that records audio from your sound card and saves it directly as MP3.

![System Audio Recorder](images/2025-02-16%2015_46_41-System%20Audio%20Recorder.png)

## Features

- Record system audio (what you hear)
- Save recordings in MP3 format
- Simple and intuitive interface
- No installation required - portable executable
- Minimal CPU usage
- Pause and resume recording
- Adjustable MP3 quality settings
- Preview recordings before saving

## Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (included)
- Working sound card/audio device

## Usage

1. Download the latest release
2. Run the application:
   - Using command line: `dotnet run --project AudioRecorder.csproj`
   - Or simply double-click the release EXE file
3. Click "Record" to begin capturing system audio
4. Use "Pause" to temporarily halt recording without stopping
5. Click "Stop" when finished
6. Preview your recording with the "Play" button
7. Click "Save" to convert and store as MP3
8. Your MP3 file will be saved in the selected output directory

## MP3 Quality Options

The application offers multiple quality presets for MP3 encoding:
- Low (128 kbps) - Smaller file size, good for voice recordings
- Medium (192 kbps) - Balanced quality and size
- High (256 kbps) - Better quality for music
- Extreme (320 kbps) - Maximum quality, larger file size

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [NAudio](https://github.com/naudio/NAudio) library for audio processing
- [LAME MP3 encoder](https://lame.sourceforge.io/) for MP3 conversion 