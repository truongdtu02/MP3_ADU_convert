using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace FFmpegLinux
{
    public class FFmpegXabe
    {
        public static string curPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public string GetDataPath(string file) => $"Data\\{file}";
        public async Task convertMP3(string nameFile)
        {
            string pathFile = GetDataPath(nameFile);
            string outPath = Path.ChangeExtension(pathFile, ".mp3");

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
                //Add audio stream to output file
                .AddStream(audioStream)
                //Set output file path
                .SetOutput(outPath)
                //SetOverwriteOutput to overwrite files. It's useful when we already run application before
                .SetOverwriteOutput(true)
                //Disable multithreading
                .UseMultiThread(false)
                //Set conversion preset. You have to chose between file size and quality of video and duration of conversion
                .SetPreset(ConversionPreset.UltraFast);
            //Add log to OnProgress
            conversion.OnProgress += async (sender, args) =>
            {
                    //Show all output from FFmpeg to console
                    await Console.Out.WriteLineAsync($"[{args.Duration}/{args.TotalLength}][{args.Percent}%]");
            };
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
