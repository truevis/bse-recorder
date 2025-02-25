# BSE Audio Recorder

A lightweight Windows application that records audio from your sound card and saves it directly as MP3.

![System Audio Recorder](images/System%20Audio%20Recorder.png)

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

## Building from Source

The project includes a `publish.bat` script that automates the build process:

1. Clone the repository
2. Run `publish.bat` from the command line
3. The script will:
   - Clean previous build files
   - Remove any duplicate source files
   - Build a self-contained, single-file executable
   - Copy the executable to the "output" folder
   - Clean up temporary build files

The resulting executable in the "output" folder is portable and doesn't require separate installation of the .NET runtime.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [NAudio](https://github.com/naudio/NAudio) library for audio processing
- [LAME MP3 encoder](https://lame.sourceforge.io/) for MP3 conversion 