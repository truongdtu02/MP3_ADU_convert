using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ADU_MP3_RFC5219
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            string filePath = @"E:\bai11.mp3";
            byte[] mp3_buff = File.ReadAllBytes(filePath).Skip(237).ToArray();
            MP3_frame mp3file = new MP3_frame(mp3_buff, mp3_buff.Length);
            SegmentQueue pendingMP3Frames = new SegmentQueue();
            List<byte[]> aduList = new List<byte[]>();
            pendingMP3Frames.MP3toADU(mp3file, aduList);

            //SegmentQueue pendingADUFrames = new SegmentQueue();
            //pendingADUFrames.ADUtoMP3(aduList);

            FileStream stream = new FileStream(@"E:\test144kbpsv1.mp3", FileMode.Append);

            //convert adu tu mp3 144kbps, backpointer = 0
            for (int i = 0; i < aduList.Count; i++)
            {
                int aduDataSize = aduList[i].Length - (4 + 17);
                byte[] frametmp = new byte[432];
                //copy header, side info
                Buffer.BlockCopy(aduList[i], 0, frametmp, 0, 4);
                Buffer.BlockCopy(aduList[i], 4, frametmp, 4, 17);
                //copy frame data
                Buffer.BlockCopy(aduList[i], 4 + 17, frametmp, 4 + 17, aduDataSize);
                //change bitrate to 144kbps
                frametmp[2] &= 0x0F;
                frametmp[2] |= 0xD0; //0b1101 000
                //change back pointer
                frametmp[4] = 0;
                //part2_3length = 0
                //frametmp[5] &= 0xC0; //0b1100 0000
                //frametmp[6] &= 0x03; //0b0000 0011
                //preFrame.Add(frametmp);
                stream.Write(frametmp, 0, frametmp.Length);
            }

            stream.Close();

            //string filePath1 = @"E:\adu.mp3";
            //byte[] mp3_buff1 = File.ReadAllBytes(filePath1);
            //string filePath2 = @"E:\aduRFC.mp3";
            //byte[] mp3_buff2 = File.ReadAllBytes(filePath2);
            //for (int i = 0; i < mp3_buff1.Length; i++)
            //{
            //    if (mp3_buff1[i] != mp3_buff2[i])
            //    {
            //        Console.WriteLine("Error");
            //    }
            //}
        }
    }
}
