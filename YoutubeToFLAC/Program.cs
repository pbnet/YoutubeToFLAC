using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Threading;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Andrei Rachita's Youtube to FLAC converter");
        Console.WriteLine("===== (C) 2024 PhoeNIXBird Networks =====");
        Console.WriteLine("");
        Console.WriteLine("Enter YouTube URL:");
        string url = Console.ReadLine();

        if (string.IsNullOrEmpty(url))
        {
            Console.WriteLine("Invalid URL");
            return;
        }

        // Initialize YoutubeExplode
        var youtube = new YoutubeClient();

        try
        {
            // Get video information
            var video = await youtube.Videos.GetAsync(url);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            string fileName = $"{video.Title}.mp4";
            Console.WriteLine($"Downloading audio from: {video.Title}");

            // Download audio stream with a progress indicator
            var progress = new Progress<double>(p => ShowProgress(p));
            await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, fileName, progress);

            Console.WriteLine($"\nDownloaded: {fileName}");

            // Convert to FLAC using FFmpeg
            string flacFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.flac";
            ConvertToFlac(fileName, flacFileName);

            Console.WriteLine($"Conversion to FLAC complete: {flacFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Exit message
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    // Progress bar display
    static void ShowProgress(double progress)
    {
        // Progress will be a double from 0.0 to 1.0, so convert it to percentage
        int progressPercentage = (int)(progress * 100);
        Console.CursorLeft = 0; // Reset cursor position on the same line
        Console.Write($"Progress: [{new string('#', progressPercentage / 2)}{new string('-', 50 - (progressPercentage / 2))}] {progressPercentage}%");
    }

    static void ConvertToFlac(string inputFilePath, string outputFilePath)
    {
        // Command to run FFmpeg
        string ffmpegCommand = $"-i \"{inputFilePath}\" \"{outputFilePath}\"";

        // Create a process to run FFmpeg
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",  // Ensure FFmpeg is in your PATH
            Arguments = ffmpegCommand,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
            process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}
