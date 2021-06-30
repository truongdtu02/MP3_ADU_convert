using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg.Downloader;

namespace FFmpegLinux
{
    public class Program
    {
        private static async Task Run()
        {
            int numOfProcess = 10;
            string fileName = "test";
            string path1 = @"Data";
            string path2 = @"Data2";
            //copy file to multiple version
            //for (int i = 1; i <= numOfProcess; i++)
            //{
            //    File.Copy(Path.Combine(path1, fileName + ".mp3"), Path.Combine(path1, fileName + i.ToString() + ".mp3"), true);
            //}
            //Thread tStream = new Thread(() => {
            //    while (FFmpegXabe.count < 2) ;

            //    //new strea file
            //    var readMp3 = new FileStream(Path.Combine(path2, fileName + "mp3"), FileMode.Open, FileAccess.Read);
            //    byte[] newBuff = new byte[10];
            //    readMp3.Read(newBuff, 0, 10);
            //    Console.WriteLine("read done");
            //});
            //tStream.Start();
            var ffmpegxabe = new FFmpegXabe();
            await ffmpegxabe.convertMP3("Data", "trường bờm hị hí hỉ hĩ" + ".mp3");
            //for (int i = 1; i <= numOfProcess; i++)
            //{
            //    await FFmpegXabe.convertMP3("Data", fileName + i.ToString() + ".mp3");
            //    //File.Copy(Path.Combine(path1, fileName + ".mp3"), Path.Combine(path1, fileName + i.ToString() + ".mp3"), true);
            //}

            //await FFmpegXabe.convertMP3("Data", "test" + ".mp3");
            //await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

            //for (int i = 1; i <= numOfProcess; i++)
            //{
            //    string iTmp = i.ToString();
            //    Thread t = new Thread(async () =>
            //    {
            //        var ffmpegxabe = new FFmpegXabe();
            //        await ffmpegxabe.convertMP3("Data", "test" + iTmp + ".mp3");
            //    });
            //    t.Start();
            //    //File.Copy(Path.Combine(path1, fileName + ".mp3"), Path.Combine(path1, fileName + i.ToString() + ".mp3"), true);
            //}
            Console.In.ReadLine();
        }
        public static async Task Main(string[] args)
        {
            Console.WriteLine("welcome to truyenthanhthongminh");
            //CreateHostBuilder(args).Build().Run();
            await Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
