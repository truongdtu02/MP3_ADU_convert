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
            for (int i = 1; i <= numOfProcess; i++)
            {
                File.Copy(Path.Combine(path1, fileName + ".mp3"), Path.Combine(path1, fileName + i.ToString() + ".mp3"), true);
            }
            //var ffmpegxabe = new FFmpegXabe();
            //await ffmpegxabe.convertMP3("Data", "test" + ".mp3");
            for (int i = 1; i <= numOfProcess; i++)
            {
                await FFmpegXabe.convertMP3("Data", fileName + i.ToString() + ".mp3");
                //File.Copy(Path.Combine(path1, fileName + ".mp3"), Path.Combine(path1, fileName + i.ToString() + ".mp3"), true);
            }

            //await FFmpegXabe.convertMP3("Data", "test" + ".mp3");

            //for (int i = 1; i <= numOfProcess; i++)
            //{
            //    int iTmp = i;
            //    Thread t = new Thread(async() =>
            //    {
            //        var ffmpegxabe = new FFmpegXabe();
            //        await ffmpegxabe.convertMP3("Data", "test" + iTmp.ToString() + ".mp3");
            //    });
            //    t.Start();
            //    //File.Copy(Path.Combine(path1, fileName + ".mp3"), Path.Combine(path1, fileName + i.ToString() + ".mp3"), true);
            //}

            Console.In.ReadLine();
        }
        public static void Main(string[] args)
        {
            Console.WriteLine("welcome to truyenthanhthongminh");
            //CreateHostBuilder(args).Build().Run();
            Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
