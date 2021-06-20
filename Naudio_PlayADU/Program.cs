using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ADU_MP3_RFC5219;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudioDemo.Mp3StreamingDemo;

namespace Naudio_PlayADU
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"E:\bai11.mp3";
            byte[] mp3_buff = File.ReadAllBytes(filePath).Skip(237).ToArray();
            MP3_frame mp3file = new MP3_frame(mp3_buff, mp3_buff.Length);
            SegmentQueue pendingMP3Frames = new SegmentQueue();
            List<byte[]> aduList = new List<byte[]>();
            pendingMP3Frames.MP3toADU(mp3file, aduList);

            //use NAudio to play each frame
            //Mp3StreamingPanel playAdu = new Mp3StreamingPanel();
            //playAdu.StreamMp3(aduList);
            //WaveStream waveStream = new Mp3FileReader(filePath);
            //var waveOut2 = new WaveOut();
            //waveOut2.Init(waveStream);
            //waveOut2.Play();

            var file = new AudioFileReader(filePath);
            var trimmed = new OffsetSampleProvider(file);
            //trimmed.SkipOver = TimeSpan.FromSeconds(15);
            //trimmed.Take = TimeSpan.FromSeconds(10);

            var player = new WaveOutEvent();
            player.Init(trimmed);
            player.Play();
        }
    }
}
