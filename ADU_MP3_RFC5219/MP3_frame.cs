using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ADU_MP3_RFC5219
{
    class MP3_frame
    {
        //constructor initialize
        protected byte[] mp3_buff = null;
        protected int mp3_buff_length = 0;
        protected bool bValidMP3 = false;
        public bool BValidMP3 { get => bValidMP3; }
        public byte[] Mp3_buff { get => mp3_buff; }
        public int Mp3_buff_length { get => mp3_buff_length; }

        //mp3 is MPEG 1 Layer III or MPEG 2 layer III, detail: https://en.wikipedia.org/wiki/MP3

        //mp3 header include 32-bit
        // byte0    byte1   byte2   byte3
        //bit 31                        0
        //detail: http://www.mp3-tech.org/programmer/frame_header.html 

        //bit 31-21 is frame sync, all bit is 1, (byte0 == FF) && (byte1 & 0xE0 == 0xE0)

        //bit 20-19 : MPEG version, 11: V1, 10: V2
        int version, version_first_header;

        public int Version { get => version; }

        //bit 18-17 Layer, just consider layer III, 01
        const int layer = 3;
        public static int Layer => layer;

        bool HaveFirstHeader = false;

        //bit 16, protected bit, don't count

        //bit 15-12 bitrate
        static readonly int[] bitrate_V1_L3 = { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 }; // MPEG 1, layer III
        static readonly int[] bitrate_V2_L3 = { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0 }; // MPEG 2, layer III
        int bitrate;
        public int Bitrate { get => bitrate; }


        //bit 11-10 sample rate
        static readonly int[] sample_rate_V1 = { 44100, 48000, 32000, 0 };
        static readonly int[] sample_rate_V2 = { 22050, 24000, 16000, 0 };
        int sample_rate, sample_rate_first_header;
        public int Sample_rate { get => sample_rate; }

        //sample per frame
        int sample_per_frame;
        static readonly int[] sample_per_frame_version = { 1152, 576 };

        public int Sample_per_frame { get => sample_per_frame; }

        //bit 9, padding bit
        int padding;
        public int Padding { get => padding; }

        int frame_size = 0;
        public int Frame_size { get => frame_size; }

        int start_frame = 0, end_frame = 0;
        double timePerFrame_ms;
        public double TimePerFrame_ms { get => timePerFrame_ms; }
        public int Start_frame { get => start_frame; }
        public int End_frame { get => end_frame; }

        int sideInfoSize;
        public int SideInfoSize { get => sideInfoSize; }

        //channel bit 7, 6; 00, 01, 10, 11 : stereo, joint stereo, dual channel, mono
        int channel;
        public int Channel { get => channel; }

        //v2 8-bit after header, v1 9-bit after header
        int main_data_begin;
        public int MainDataBegin { get => main_data_begin; }

        int totalFrame = 0;
        public int TotalFrame { get => totalFrame; }

        private bool IsValidHeader(byte[] buff, int i_buff, int buff_length)
        {
            //get infor header
            int header = (int)buff[i_buff + 3] | ((int)buff[i_buff + 2] << 8) | ((int)buff[i_buff + 1] << 16) | ((int)buff[i_buff] << 24);

            //get version
            int tmp = (header >> 19) & 0b11;
            if (tmp == 0b11)
                version = 1;
            else if (tmp == 0b10)
                version = 2;
            else
                return false;

            //get layer
            tmp = (header >> 17) & 0b11;
            if (tmp != 0b01) //layer III
                return false;

            //get bitrate
            tmp = (header >> 12) & 0b1111;
            if ((tmp == 0) || (tmp == 0b1111))
                return false;
            if (version == 1)
                bitrate = bitrate_V1_L3[tmp];
            else if (version == 2)
                bitrate = bitrate_V2_L3[tmp];

            //get smaple rate
            tmp = (header >> 10) & 0b11;
            if (tmp == 0b11)
                return false;
            if (version == 1)
                sample_rate = sample_rate_V1[tmp];
            else if (version == 2)
                sample_rate = sample_rate_V2[tmp];

            //check, if it is next frame, compare with first frame
            if (!HaveFirstHeader) //it is first frame
            {
                version_first_header = version;
                sample_rate_first_header = sample_rate;
                HaveFirstHeader = true;
            }
            else //next frame
            {
                if ((version_first_header != version) || (sample_rate_first_header != sample_rate))
                    return false;
            }

            //get padding
            padding = (header >> 9) & 1;

            //get sample per frame
            sample_per_frame = sample_per_frame_version[version - 1];

            //timePerFrame_ms
            timePerFrame_ms = 1000.0 * (double)sample_per_frame / (double)sample_rate;

            //get frame size
            double frame_size_tmp = bitrate * 1000 * sample_per_frame / 8 / sample_rate + padding;
            frame_size = (int)frame_size_tmp;

            //check next frame
            if ((i_buff + frame_size) > buff_length) //out of range
            {
                return false;
            }

            return true;
        }

        private void GetNextFrameSize(int i_buff)
        {
            //get infor header
            int header = (int)mp3_buff[i_buff + 3] | ((int)mp3_buff[i_buff + 2] << 8) | ((int)mp3_buff[i_buff + 1] << 16) | ((int)mp3_buff[i_buff] << 24);

            //get version
            int tmp = (header >> 19) & 0b11;
            if (tmp == 0b11)
                version = 1;
            else if (tmp == 0b10)
                version = 2;

            //get layer
            tmp = (header >> 17) & 0b11;

            //get bitrate
            tmp = (header >> 12) & 0b1111;

            if (version == 1)
                bitrate = bitrate_V1_L3[tmp];
            else if (version == 2)
                bitrate = bitrate_V2_L3[tmp];

            //get smaple rate
            tmp = (header >> 10) & 0b11;

            if (version == 1)
                sample_rate = sample_rate_V1[tmp];
            else if (version == 2)
                sample_rate = sample_rate_V2[tmp];

            //get padding
            padding = (header >> 9) & 1;

            //get sample per frame
            sample_per_frame = sample_per_frame_version[version - 1];

            //timePerFrame_ms
            timePerFrame_ms = 1000.0 * (double)sample_per_frame / (double)sample_rate;

            //get frame size
            double frame_size_tmp = bitrate * 1000 * sample_per_frame / 8 / sample_rate + padding;
            frame_size = (int)frame_size_tmp;

            //get channel
            channel = (header >> 6) & 3; //0b11

            //get side info size
            if (version == 1 && channel != 3) // v1, stereo
            {
                sideInfoSize = 32;
            }
            else if (version == 2 && channel == 3) //v2, mono
            {
                sideInfoSize = 9;
            }
            else
            {
                sideInfoSize = 17;
            }

            //get main data begin
            if (version == 1)
            {
                main_data_begin = ((int)mp3_buff[4 + i_buff] << 1) | (((int)mp3_buff[5 + i_buff] >> 7) & 1); // 9-bit
            }
            else
            {
                main_data_begin = (int)mp3_buff[4 + i_buff]; // 8-bit
            }
        }

        protected bool IsValidMp3()
        {
            if ((mp3_buff == null) || (mp3_buff_length < 1)) //invalid initialize
            {
                return false;
            }

            int index_buff_mp3 = 0, numOfFrame = 0;

            while (index_buff_mp3 < (mp3_buff_length - 3)) // a frame has at least 4 bytes
            {
                if ((mp3_buff[index_buff_mp3] == 0xFF) && ((mp3_buff[index_buff_mp3 + 1] & 0xE0) == 0xE0)) //sync bit
                {
                    if (IsValidHeader(mp3_buff, index_buff_mp3, mp3_buff_length))
                    {
                        index_buff_mp3 += frame_size;
                        numOfFrame++;
                        continue;
                    }
                }
                else
                {
                    if (HaveFirstHeader) return false;
                }
                index_buff_mp3++;
            }
            totalFrame = numOfFrame;
            return true;
        }

        bool bReadNextFrameFirst = true;
        //use this method, start_frame will point to next frame and update frame_size, bitrate, time per frame,... of new frame
        public bool ReadNextFrame()
        {
            if (!bValidMP3) //wrong mp3
            {
                return false;
            }

            if (bReadNextFrameFirst)
            {
                start_frame = 0;
                bReadNextFrameFirst = false;
            }
            else
            {
                start_frame += frame_size;
                if (start_frame >= mp3_buff_length) return false; //out of range
            }
            GetNextFrameSize(start_frame);
            return true;
        }

        //constructor
        public MP3_frame(byte[] _Mp3_buff, int _Mp3_buff_length)
        {
            mp3_buff = _Mp3_buff;
            mp3_buff_length = _Mp3_buff_length;

            bValidMP3 = IsValidMp3();
        }
        public MP3_frame()
        {
            mp3_buff = null;
            mp3_buff_length = 0;
        }
    }


    class ADU_frame : MP3_frame
    {
        //byte[] adu_save = new byte[511];
        int adu_save_size = 0;
        const int adu_max = 511;
        List<byte[]> list_adu_save = new List<byte[]>();

        public byte[] ReadNextADUFrame()
        {
            if (ReadNextFrame())
            {
                int byte_array_size_tmp = Frame_size - 4 - SideInfoSize;
                byte[] byte_array_tmp = new byte[byte_array_size_tmp];
                Buffer.BlockCopy(Mp3_buff, Start_frame + 4 + SideInfoSize, byte_array_tmp, 0, byte_array_size_tmp);
                adu_save_size += byte_array_size_tmp;
                list_adu_save.Add(byte_array_tmp);
                if (list_adu_save.Count > 0 && (adu_save_size - list_adu_save[0].Length) > adu_max)
                {
                    list_adu_save.RemoveAt(0);
                }

                byte[] ADU_frame_tmp = new byte[Frame_size + MainDataBegin];
                //copy frame header
                Buffer.BlockCopy(Mp3_buff, Start_frame, ADU_frame_tmp, 0, 4);
                //copy side infor
                Buffer.BlockCopy(Mp3_buff, Start_frame + 4, ADU_frame_tmp, 4, SideInfoSize);

                //clear main_data_begin = 0
                if (Version == 1) //9-bit
                {
                    ADU_frame_tmp[4] = 0;
                    ADU_frame_tmp[5] &= 0x7F; //0b 0111 1111
                }
                else //8-bit
                {
                    ADU_frame_tmp[4] = 0;
                }
                //copy frame_data
                Buffer.BlockCopy(Mp3_buff, Start_frame + 4 + SideInfoSize, ADU_frame_tmp, 4 + SideInfoSize + MainDataBegin, Frame_size - 4 - SideInfoSize);

                //copy main_data_begin
                int index_adu_list = list_adu_save.Count - 1;
                int adu_offset = MainDataBegin;
                while (index_adu_list > -1)
                {
                    if (adu_offset > list_adu_save[index_adu_list].Length)
                    {
                        adu_offset -= list_adu_save[index_adu_list].Length;
                        Buffer.BlockCopy(list_adu_save[index_adu_list], 0, ADU_frame_tmp, 4 + SideInfoSize + adu_offset, list_adu_save[index_adu_list].Length);
                    }
                    else if (adu_offset > 0)
                    {
                        Buffer.BlockCopy(list_adu_save[index_adu_list], list_adu_save[index_adu_list].Length - adu_offset, ADU_frame_tmp, 4 + SideInfoSize, adu_offset);
                        break;
                    }
                    index_adu_list--;
                }
                return ADU_frame_tmp;

            }
            else
            {
                return null;
            }
        }

        //put main_data_begin first
        public byte[] ReadNextADUFrame2()
        {
            if (ReadNextFrame())
            {
                int byte_array_size_tmp = Frame_size - 4 - SideInfoSize;
                byte[] byte_array_tmp = new byte[byte_array_size_tmp];
                Buffer.BlockCopy(Mp3_buff, Start_frame + 4 + SideInfoSize, byte_array_tmp, 0, byte_array_size_tmp);
                adu_save_size += byte_array_size_tmp;
                list_adu_save.Add(byte_array_tmp);
                if (list_adu_save.Count > 0 && (adu_save_size - list_adu_save[0].Length) > adu_max)
                {
                    list_adu_save.RemoveAt(0);
                }

                byte[] ADU_frame_tmp = new byte[Frame_size + MainDataBegin];
                //copy frame header
                Buffer.BlockCopy(Mp3_buff, Start_frame, ADU_frame_tmp, MainDataBegin, 4);
                //copy side infor
                Buffer.BlockCopy(Mp3_buff, Start_frame + 4, ADU_frame_tmp, 4 + MainDataBegin, SideInfoSize);

                //clear main_data_begin = 0
                if (Version == 1) //9-bit
                {
                    ADU_frame_tmp[4 + MainDataBegin] = 0;
                    ADU_frame_tmp[5 + MainDataBegin] &= 0x7F; //0b 0111 1111
                }
                else //8-bit
                {
                    ADU_frame_tmp[4 + MainDataBegin] = 0;
                }
                //copy frame_data
                Buffer.BlockCopy(Mp3_buff, Start_frame + 4 + SideInfoSize, ADU_frame_tmp, 4 + SideInfoSize + MainDataBegin, Frame_size - 4 - SideInfoSize);

                //copy main_data_begin
                int index_adu_list = list_adu_save.Count - 1;
                int adu_offset = MainDataBegin;
                while (index_adu_list > -1)
                {
                    if (adu_offset > list_adu_save[index_adu_list].Length)
                    {
                        adu_offset -= list_adu_save[index_adu_list].Length;
                        Buffer.BlockCopy(list_adu_save[index_adu_list], 0, ADU_frame_tmp, adu_offset, list_adu_save[index_adu_list].Length);
                    }
                    else if (adu_offset > 0)
                    {
                        Buffer.BlockCopy(list_adu_save[index_adu_list], list_adu_save[index_adu_list].Length - adu_offset, ADU_frame_tmp, 0, adu_offset);
                        break;
                    }
                    index_adu_list--;
                }
                return ADU_frame_tmp;

            }
            else
            {
                return null;
            }
        }


        //constructor
        public ADU_frame(byte[] _Mp3_buff, int _Mp3_buff_length)
        {
            mp3_buff = _Mp3_buff;
            mp3_buff_length = _Mp3_buff_length;

            bValidMP3 = IsValidMp3();
        }
        public ADU_frame()
        {
            mp3_buff = null;
            mp3_buff_length = 0;
        }
    }

    /*
     *   -  "Segment": A record that represents either a "MP3 Frame" or an
      "ADU Frame".  It consists of the following fields:
      -  "header": the 4-byte MPEG header
      -  "headerSize": a constant (== 4)
      -  "sideInfo": the 'side info' structure, *including* the optional
         2-byte CRC field, if present
      -  "sideInfoSize": the size (in bytes) of the above structure
      -  "frameData": the remaining data in this frame
      -  "frameDataSize": the size (in bytes) of the above data
      -  "backpointer": the value (expressed in bytes) of the
         backpointer for this frame
      -  "aduDataSize": the size (in bytes) of the ADU associated with
         this frame.  (If the frame is already an "ADU Frame", then
         aduDataSize == frameDataSize)
      -  "mp3FrameSize": the total size (in bytes) that this frame would
         have if it were a regular "MP3 Frame".  (If it is already a
         "MP3 Frame", then mp3FrameSize == headerSize + sideInfoSize +
         frameDataSize) Note that this size can be derived completely
         from "header".

        
     */

    class Segment //mp3 frame or adu frame
    {
        //bit 15-12 bitrate
        static readonly int[] bitrate_V1_L3 = { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 }; // MPEG 1, layer III
        static readonly int[] bitrate_V2_L3 = { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0 }; // MPEG 2, layer III


        //bit 11-10 sample rate
        static readonly int[] sample_rate_V1 = { 44100, 48000, 32000, 0 };
        static readonly int[] sample_rate_V2 = { 22050, 24000, 16000, 0 };

        static readonly int[] sample_per_frame_version = { 1152, 576 };

        public byte[] header = new byte[4];
        public int headerSize = 4;
        public byte[] sideInfo; //no CRC
        public int sideInfoSize;
        public byte[] frameData;
        public int frameDataSize, backpointer, aduDataSize, mp3FrameSize;

        public int version, bitrate, sample_rate, padding, sample_per_frame, channel;
        public double timePerFrame_ms;

        public Segment(MP3_frame mp3file)
        {
            //copy header
            Buffer.BlockCopy(mp3file.Mp3_buff, mp3file.Start_frame, header, 0, 4);
            //copy side infor
            sideInfoSize = mp3file.SideInfoSize;
            sideInfo = new byte[sideInfoSize];
            Buffer.BlockCopy(mp3file.Mp3_buff, mp3file.Start_frame + 4, sideInfo, 0, sideInfoSize);
            //copy frame data
            frameDataSize = mp3file.Frame_size - 4 - sideInfoSize;
            frameData = new byte[frameDataSize];
            Buffer.BlockCopy(mp3file.Mp3_buff, mp3file.Start_frame + 4 + sideInfoSize, frameData, 0, frameDataSize);

            mp3FrameSize = mp3file.Frame_size;
            backpointer = mp3file.MainDataBegin;
            aduDataSize = backpointer + frameDataSize;
        }

        public Segment(byte[] aduFrame)
        {
            //check header adu frame
            //get infor header

            if (aduFrame.Length >= 4)
            {
                int int_header = (int)aduFrame[3] | ((int)aduFrame[2] << 8) | ((int)aduFrame[1] << 16) | ((int)aduFrame[0] << 24);
                //get version
                int tmp = (int_header >> 19) & 0b11;
                if (tmp == 0b11)
                    version = 1;
                else if (tmp == 0b10)
                    version = 2;
                else
                    return;

                //get layer
                tmp = (int_header >> 17) & 0b11;
                if (tmp != 0b01) //layer III
                    return;

                //get bitrate
                tmp = (int_header >> 12) & 0b1111;
                if ((tmp == 0) || (tmp == 0b1111))
                    return;
                if (version == 1)
                    bitrate = bitrate_V1_L3[tmp];
                else if (version == 2)
                    bitrate = bitrate_V2_L3[tmp];

                //get smaple rate
                tmp = (int_header >> 10) & 0b11;
                if (tmp == 0b11)
                    return;
                if (version == 1)
                    sample_rate = sample_rate_V1[tmp];
                else if (version == 2)
                    sample_rate = sample_rate_V2[tmp];

                //get padding
                padding = (int_header >> 9) & 1;

                //get sample per frame
                sample_per_frame = sample_per_frame_version[version - 1];

                //timePerFrame_ms
                timePerFrame_ms = 1000.0 * (double)sample_per_frame / (double)sample_rate;

                //get frame size
                double frame_size_tmp = bitrate * 1000 * sample_per_frame / 8 / sample_rate + padding;
                mp3FrameSize = (int)frame_size_tmp;

                //get channel
                channel = (int_header >> 6) & 3; //0b11

                //get side info size
                if (version == 1 && channel != 3) // v1, stereo
                {
                    sideInfoSize = 32;
                }
                else if (version == 2 && channel == 3) //v2, mono
                {
                    sideInfoSize = 9;
                }
                else
                {
                    sideInfoSize = 17;
                }

                //get main data begin
                if (version == 1)
                {
                    backpointer = ((int)aduFrame[4] << 1) | (((int)aduFrame[5] >> 7) & 1); // 9-bit
                }
                else
                {
                    backpointer = (int)aduFrame[4]; // 8-bit
                }

                if (backpointer + mp3FrameSize != aduFrame.Length) return;
            }
            else
            {
                return;
            }

            //copy header
            Buffer.BlockCopy(aduFrame, 0, header, 0, 4);
            //copy side infor
            sideInfo = new byte[sideInfoSize];
            Buffer.BlockCopy(aduFrame, 4, sideInfo, 0, sideInfoSize);
            //copy frame data
            frameDataSize = aduFrame.Length - 4 - sideInfoSize;
            frameData = new byte[frameDataSize];
            Buffer.BlockCopy(aduFrame, 4 + sideInfoSize, frameData, 0, frameDataSize);

            aduDataSize = frameDataSize;
        }
    }

    /*
     *  -  "SegmentQueue": A FIFO queue of "Segments", with operations
      -  void enqueue(Segment)
      -  Segment dequeue()
      -  Boolean isEmpty()
      -  Segment head()
      -  Segment tail()
      -  Segment previous(Segment):  returns the segment prior to a
         given one
      -  Segment next(Segment): returns the segment after a given one
      -  unsigned totalDataSize(): returns the sum of the
         "frameDataSize" fields of each entry in the queue
     */

    class SegmentQueue
    {
        //const int totalDataSizeMax = 511; //2^9 - 1
        public List<Segment> segmentqueue = new List<Segment>();
        public int totalDataSize = 0;
        public void enqueue(Segment _segment)
        {
            segmentqueue.Add(_segment);
            totalDataSize += _segment.frameDataSize;
        }

        public Segment dequeue() //return and remove first element
        {
            if (segmentqueue.Count > 0)
            {
                Segment tmp = segmentqueue[0];
                totalDataSize -= tmp.frameDataSize;
                if (totalDataSize < 0) totalDataSize = 0; //for sure
                segmentqueue.RemoveAt(0);
                return tmp;
            }
            return null;
        }
        public bool isEmpty()
        {
            if (segmentqueue.Count > 0)
                return false;
            else
                return true;
        }

        public Segment head()
        {
            if (segmentqueue.Count > 0)
            {
                return segmentqueue[0]; //first element
            }
            return null;
        }

        public Segment tail()
        {
            if (segmentqueue.Count > 0)
            {
                return segmentqueue[segmentqueue.Count - 1]; //last element
            }
            return null;
        }

        public Segment previous(Segment _segment)
        {
            int indexOfCur = segmentqueue.IndexOf(_segment);
            if (indexOfCur == -1) return null;
            if (indexOfCur > 0)
            {
                return segmentqueue[indexOfCur - 1];
            }
            else
            {
                return null;
            }
        }

        public Segment next(Segment _segment)
        {
            int indexOfCur = segmentqueue.IndexOf(_segment);
            if (indexOfCur == -1) return null;
            if (indexOfCur < segmentqueue.Count - 1) //index <= segmentqueue.Count - 2
            {
                return segmentqueue[indexOfCur + 1];
            }
            else
            {
                return null;
            }
        }

        public void MP3toADU(MP3_frame mp3file, List<byte[]> aduList)
        {
            //SegmentQueue pendingMP3Frames; // initially empty
            //output
            //FileStream stream = new FileStream(@"E:\aduRFC.mp3", FileMode.Append);
            while (true)
            {
                // Enqueue new MP3 Frames, until we have enough data to
                // generate the ADU for a frame:
                int totalDataSizeBefore;
                Segment newFrame;
                bool b_endOfMp3File = false;
                do
                {
                    totalDataSizeBefore = totalDataSize;
                    //Segment newFrame;// = 'the next MP3 Frame';
                    if (mp3file.ReadNextFrame())
                    {
                        newFrame = new Segment(mp3file);
                        enqueue(newFrame);
                    }
                    else
                    {
                        b_endOfMp3File = true;
                        break;
                    }
                } while (totalDataSizeBefore < newFrame.backpointer);

                if (b_endOfMp3File) break;

                // We now have enough data to generate the ADU for the most
                // recently enqueued frame (i.e., the tail of the queue).
                // (The earlier frames in the queue -- if any -- must be
                // discarded, as we don't have enough data to generate
                // their ADUs.)
                Segment tailFrame = tail();
                byte[] aduFrame_tmp = new byte[tailFrame.mp3FrameSize + tailFrame.backpointer];
                int aduFrame_tmpIndex = 0;
                // Output the header and side info:
                //output(tailFrame.header);
                Buffer.BlockCopy(tailFrame.header, 0, aduFrame_tmp, aduFrame_tmpIndex, tailFrame.headerSize);
                aduFrame_tmpIndex += tailFrame.headerSize;
                //output(tailFrame.sideInfo);
                Buffer.BlockCopy(tailFrame.sideInfo, 0, aduFrame_tmp, aduFrame_tmpIndex, tailFrame.sideInfoSize);
                aduFrame_tmpIndex += tailFrame.sideInfoSize;

                // Go back to the frame that contains the start of our
                // ADU data:
                int offset = 0;
                Segment curFrame = tailFrame;
                int prevBytes = tailFrame.backpointer;
                while (prevBytes > 0)
                {
                    curFrame = previous(curFrame);
                    int dataHere = curFrame.frameDataSize;
                    if (dataHere < prevBytes)
                    {
                        prevBytes -= dataHere;
                    }
                    else
                    {
                        offset = dataHere - prevBytes;
                        break;
                    }
                }

                // Dequeue any frames that we no longer need:
                while (head() != curFrame)
                {
                    dequeue();
                }

                // Output, from the remaining frames, the ADU data that
                // we want:
                int bytesToUse = tailFrame.aduDataSize;
                while (bytesToUse > 0)
                {
                    int dataHere = curFrame.frameDataSize - offset;
                    int bytesUsedHere = dataHere < bytesToUse ? dataHere : bytesToUse;

                    //output("bytesUsedHere" bytes from curFrame.frameData, starting from "offset");
                    Buffer.BlockCopy(curFrame.frameData, offset, aduFrame_tmp, aduFrame_tmpIndex, bytesUsedHere);
                    aduFrame_tmpIndex += bytesUsedHere;

                    bytesToUse -= bytesUsedHere;
                    offset = 0;
                    curFrame = next(curFrame);
                }
                aduList.Add(aduFrame_tmp);
                //stream.Write(aduFrame_tmp, 0, aduFrame_tmp.Length);
            }
            //stream.Close();
        }
        public FileStream stream;// = new FileStream(@"E:\aduRFC.mp3", FileMode.Append);
        public void ADUtoMP3(List<byte[]> aduList)
        {
            //SegmentQueue pendingADUFrames; // initially empty = 0;
            //output
            stream = new FileStream(@"E:\aduRFC.mp3", FileMode.Append);
            int aduListIndex = 0;
            while (true)
            {
                while (needToGetAnADU())
                {
                    //Segment newADU = 'the next ADU Frame';
                    if(aduListIndex < aduList.Count)
                    {
                        Segment newADU = new Segment(aduList[aduListIndex++]);
                        enqueue(newADU);

                        insertDummyADUsIfNecessary();
                    }
                }

                generateFrameFromHeadADU();
            }
            stream.Close();
        }

        bool needToGetAnADU()
        {
            // Checks whether we need to enqueue one or more new ADUs
            // before we have enough data to generate a frame for the
            // head ADU.
            bool needToEnqueue = true;

            if (!isEmpty())
            {
                Segment curADU = head();
                int endOfHeadFrame = curADU.mp3FrameSize - curADU.headerSize - curADU.sideInfoSize;
                int frameOffset = 0;

                while (true)
                {
                    int endOfData = frameOffset - curADU.backpointer + curADU.aduDataSize;
                    if (endOfData >= endOfHeadFrame)
                    {
                        // We have enough data to generate a
                        // frame.
                        needToEnqueue = false;
                        break;
                    }

                    frameOffset += curADU.mp3FrameSize - curADU.headerSize - curADU.sideInfoSize;
                    if (curADU == tail()) break;
                    curADU = next(curADU);
                }
            }
            return needToEnqueue;
        }

        void generateFrameFromHeadADU()
        {
            Segment curADU = head();

            // Output the header and side info:
            //output(curADU.header);
            stream.Write(curADU.header, 0, curADU.header.Length);
            //output(curADU.sideInfo);
            stream.Write(curADU.sideInfo, 0, curADU.sideInfo.Length);

            // Begin by zeroing out the rest of the frame, in case the
            // ADU data doesn't fill it in completely:
            int endOfHeadFrame = curADU.mp3FrameSize - curADU.headerSize - curADU.sideInfoSize;
            //output("endOfHeadFrame" zero bytes);
            byte[] tmp_zero_array = new byte[endOfHeadFrame];
            stream.Write(tmp_zero_array, 0, tmp_zero_array.Length);

            // Fill in the frame with appropriate ADU data from this and
            // subsequent ADUs:
            int frameOffset = 0;
            int toOffset = 0;

            while (toOffset < endOfHeadFrame)
            {
                int startOfData = frameOffset - curADU.backpointer;
                if (startOfData > endOfHeadFrame)
                {
                    break; // no more ADUs are needed
                }
                int endOfData = startOfData + curADU.aduDataSize;
                if (endOfData > endOfHeadFrame)
                {
                    endOfData = endOfHeadFrame;
                }

                int fromOffset;
                if (startOfData <= toOffset)
                {
                    fromOffset = toOffset - startOfData;
                    startOfData = toOffset;
                    if (endOfData < startOfData)
                    {
                        endOfData = startOfData;
                    }
                }
                else
                {
                    fromOffset = 0;

                    // leave some zero bytes beforehand:
                    toOffset = startOfData;
                }

                int bytesUsedHere = endOfData - startOfData;
                //output(starting at offset "toOffset", "bytesUsedHere" bytes from "&curADU.frameData[fromOffset]");
                stream.Write(curADU.frameData, toOffset + frameOffset, bytesUsedHere);
                toOffset += bytesUsedHere;

                frameOffset += curADU.mp3FrameSize - curADU.headerSize - curADU.sideInfoSize;
                curADU = next(curADU);
            }

            dequeue();
        }

        void insertDummyADUsIfNecessary()
        {
            // The tail segment (ADU) is assumed to have been recently
            // enqueued.  If its backpointer would overlap the data
            // of the previous ADU, then we need to insert one or more
            // empty, 'dummy' ADUs ahead of it.  (This situation
            // should occur only if an intermediate ADU was missing
            // -- e.g., due to packet loss.)
            while (true)
            {
                Segment tailADU = tail();
                int prevADUend; // relative to the start of the tail ADU

                if (head() != tailADU)
                {
                    // there is a previous ADU
                    Segment prevADU = previous(tailADU);
                    prevADUend = prevADU.mp3FrameSize + prevADU.backpointer - prevADU.headerSize - prevADU.sideInfoSize;
                    if (prevADU.aduDataSize > prevADUend)
                    {
                        // this shouldn't happen if the
                        // previous ADU was well-formed
                        prevADUend = 0;
                    }
                    else
                    {
                        prevADUend -= prevADU.aduDataSize;
                    }
                }
                else
                {
                    prevADUend = 0;
                }

                if (tailADU.backpointer > prevADUend)
                {
                    // Insert a 'dummy' ADU in front of the tail.
                    // This ADU can have the same "header" (and thus,
                    // "mp3FrameSize") as the tail ADU, but should
                    // have a "backpointer" of "prevADUend", and
                    // an "aduDataSize" of zero.  The simplest
                    // way to do this is to copy the "sideInfo" from
                    // the tail ADU, replace the value of
                    // "main_data_begin" with "prevADUend", and set
                    // all of the "part2_3_length" fields to zero.
                }
                else
                {
                    break; // no more dummy ADUs need to be
                           // inserted
                }
            }
        }


    }
}

