using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MP3_ADU
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string filePath = @"E:\bai111.mp3";
            byte[] mp3_buff = File.ReadAllBytes(filePath).Skip(237).ToArray();
            MP3_ADU mp3file = new MP3_ADU(mp3_buff, mp3_buff.Length);
            int numFrame = 0;



            ADU_frame adufile = new ADU_frame(mp3_buff, mp3_buff.Length);
            int aduNumFrame = 0;

            //FileStream stream = new FileStream(@"E:\test10.mp3", FileMode.Append);

            List<byte[]> preFrame = new List<byte[]>();
            while (true)
            {
                byte[] aduframe = adufile.ReadNextADUFrame();
                if (aduframe != null)
                {
                    aduNumFrame++;
                    //AppendAllBytes(@"E:\adu.mp3", aduframe);
                    //stream.Write(aduframe, 0, aduframe.Length);
                }
                else
                {
                    break;
                }

                if (aduNumFrame == 6809)
                {
                    int backPoint = aduframe.Length - 144;
                    int offset = 0;
                    byte[] frametmp = new byte[288];
                    offset = 288 - backPoint;
                    //copy header, side info
                    Buffer.BlockCopy(aduframe, 0, frametmp, 0, 4);
                    Buffer.BlockCopy(aduframe, 4, frametmp, 4, 17);
                    //copy frame data
                    Buffer.BlockCopy(aduframe, 4 + 17, frametmp, offset, backPoint);
                    //change bitrate to 96kbps
                    frametmp[2] &= 0x0F;
                    frametmp[2] |= 0xA0;
                    //change back pointer
                    frametmp[4] = 0;
                    //part2_3length = 0
                    frametmp[5] &= 0xC0; //0b1100 0000
                    frametmp[6] &= 0x03; //0b0000 0011
                    preFrame.Add(frametmp);
                }

            }
            //stream.Close();

            FileStream stream = new FileStream(@"E:\test10frame3.mp3", FileMode.Append);
            for(int i = preFrame.Count - 1; i > -1; i--)
            {
                stream.Write(preFrame[i], 0, preFrame[i].Length);
            }

            while (mp3file.ReadNextFrame())
            {
                numFrame++;

                if (numFrame >= 6809)
                {
                    byte[] frametmp = new byte[mp3file.Frame_size];
                    Buffer.BlockCopy(mp3_buff, mp3file.Start_frame, frametmp, 0, mp3file.Frame_size);
                    stream.Write(frametmp, 0, frametmp.Length);
                }

            }
            stream.Close();
            Console.WriteLine("DONE");
        }

        public static void AppendAllBytes(string path, byte[] bytes)
        {
            //argument-checking here.

            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}

/*
 *if(aduNumFrame == 6809)
                {
                    int backPoint = aduframe.Length - 144;
                    int prevByte = backPoint;
                    int offset = 0;
                    while(prevByte > 0)
                    {
                        byte[] frametmp = new byte[144];
                        int byteUsedHere;
                        if(prevByte > 123)
                        {
                            byteUsedHere = 123;
                            offset = 0;
                        }
                        else
                        {
                            byteUsedHere = prevByte;
                            offset = 123 - byteUsedHere;
                        }
                        prevByte -= byteUsedHere;
                        //copy header, side info
                        Buffer.BlockCopy(aduframe, 0, frametmp, 0, 4);
                        Buffer.BlockCopy(aduframe, 4, frametmp, 4, 17);
                        //copy frame data
                        Buffer.BlockCopy(aduframe, 4 + 17 + prevByte, frametmp, 4 + 17 + offset, byteUsedHere);
                        //change back pointer
                        frametmp[4] = (byte)prevByte;
                        //part2_3length = 0
                        frametmp[5] &= 0xC0; //0b1100 0000
                        frametmp[6] &= 0x03; //0b0000 0011
                        preFrame.Add(frametmp);
                    }
                }
 */
