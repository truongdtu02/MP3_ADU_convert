using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace FFmpegLinux
{
    public class FFmpegXabe
    {
        //public static string curPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static int count = 0;
        public static FileStream fs;// = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        public async Task convertMP3(string pathFile, string nameFile)
        {
            //Get latest version of FFmpeg. It's great idea if you don't know if you had installed FFmpeg.
            //await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

            pathFile = Path.Combine(pathFile, nameFile);
            string outPath = Path.Combine(@"Data2", nameFile);
            outPath = Path.ChangeExtension(outPath, ".mp3");
            //fs = new FileStream(outPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            //string outPath = "converted"; // Your code goes here
            //outPath = Path.Combine(curPath, outPath, nameFile);

            //Save file to the same location with changed extension
            //string outputFileName = nameFile + ".mp3";//Path.ChangeExtension(nameFile, ".mp4");

            var mediaInfo = await FFmpeg.GetMediaInfo(pathFile);
            
            //var videoStream = mediaInfo.VideoStreams.First();
            var audioStream = mediaInfo.AudioStreams.First();

            //Change some parameters of video stream
            audioStream
                .SetBitrate(48000)
                .SetChannels(1)
                .SetSampleRate(24000)
                ;
            //Create new conversion object
            var conversion = FFmpeg.Conversions.New()
                .AddParameter("-re", ParameterPosition.PreInput)
                //Add audio stream to output file
                .AddStream(audioStream)
                //Set output file path
                //.SetOutput(outPath)
                //SetOverwriteOutput to overwrite files. It's useful when we already run application before
                //.SetOverwriteOutput(true)
                //Disable multithreading
                .UseMultiThread(true)
                //Set conversion preset. You have to chose between file size and quality of video and duration of conversion
                .SetPreset(ConversionPreset.UltraFast)
            .AddParameter("-f mp3 udp://127.0.0.1:11000");
            //Add log to OnProgress
            conversion.OnProgress += async (sender, args) =>
            {
                //Show all output from FFmpeg to console
                await Console.Out.WriteLineAsync($"[{args.Duration}/{args.TotalLength}][{args.Percent}%] {nameFile}");
                count++;
            };

            //conversion.OnProgress += (duration, length) => { currentProgress = duration; }

            //conversion.OnDataReceived += (sender, args) =>
            //{
            //    Console.WriteLine($"{args.Data}{sender.ToString()}");
            //    count++;
            //};
            //Start conversion
            await conversion.Start();

            //await Console.Out.WriteLineAsync($"Finished converion file [{nameFile}]");
        }







        ////////////////////
        ///



        //private async Task<MediaMetadata> GetVideoThumbnailAsync(IFormFile file, int frameTarget)
        //{
        //    var fileName = file.FileName;
        //    var filePath = Path.Combine(_rootPath, "videos", fileName);
        //    var fileExtension = Path.GetExtension(filePath);

        //    // the xabe wrapper works with only mp4 extension to create thumbnail , if the file is any other format first convert it to
        //    //the mp4 format and then goahead with creating the thumbnail.  
        //    var thumbnailImageName = fileName.Replace(fileExtension, ".jpg");
        //    var thumbnailImagePath = Path.Combine(_rootPath, "thumbnails", thumbnailImageName);

        //    using (Stream fileStream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(fileStream);
        //    }
        //    Console.WriteLine(Path.Combine(_rootPath, "ffmpeg"));
        //    FFmpeg.SetExecutablesPath(Path.Combine(_rootPath, "ffmpeg"));
        //    IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(filePath);
        //    var videoDuration = mediaInfo.VideoStreams.First().Duration;
        //    IConversion conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(filePath, thumbnailImagePath, TimeSpan.FromSeconds(frameTarget));
        //    IConversionResult result = await conversion.Start();
        //    MediaMetadata media = new MediaMetadata();
        //    media.DurationSeconds = Convert.ToInt32(videoDuration.TotalMilliseconds);
        //    // media.DurationSeconds=10;
        //    media.ThumbnailImagePath = thumbnailImagePath;
        //    return media;

        //}
    }
}
