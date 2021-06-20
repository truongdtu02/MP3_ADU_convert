using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using ADU_MP3_RFC5219;
using NAudio.Wave;

namespace NAudioDemo.Mp3StreamingDemo
{
    public partial class Mp3StreamingPanel
    {
        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }


        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private HttpWebRequest webRequest;
        private VolumeWaveProvider16 volumeProvider;

        delegate void ShowErrorDelegate(string message);

        public int frameID = 0;

        public void StreamMp3(List<byte[]> aduList)
        {
            fullyDownloaded = false;

            //var readFullyStream = new ReadFullyStream();

            var buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

            IMp3FrameDecompressor decompressor = null;
            try
            {
                do
                {
                    MemoryStream MemStream = new MemoryStream(aduList[frameID]);

                    //MemStream.Write(aduList[frameID], 0, aduList[frameID].Length);
                    Segment segmentADU = new Segment(aduList[frameID]);

                    frameID++;

                    if (IsBufferNearlyFull)
                    {
                        Debug.WriteLine("Buffer getting full, taking a break");
                        Thread.Sleep(500);
                    }
                    else
                    {
                        Mp3Frame frame;// new = Mp3Frame((Stream)MemStream);
                        try
                        {
                            frame = Mp3Frame.LoadFromStream((Stream)MemStream);
                            //frame = new Mp3Frame((Stream)MemStream);
                            
                        }
                        catch (EndOfStreamException)
                        {
                            fullyDownloaded = true;
                            // reached the end of the MP3 file / stream
                            break;
                        }
                        catch (WebException)
                        {
                            // probably we have aborted download from the GUI thread
                            break;
                        }
                        if (frame == null) break;
                        if (decompressor == null)
                        {
                            // don't think these details matter too much - just help ACM select the right codec
                            // however, the buffered provider doesn't know what sample rate it is working at
                            // until we have a frame
                            decompressor = CreateFrameDecompressor(frame);
                            bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                            bufferedWaveProvider.BufferDuration =
                                TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves
                                                          //this.bufferedWaveProvider.BufferedDuration = 250;
                        }
                        int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                        //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                        bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                        if (frameID == 2) Play();
                    }

                } while (true);
                Debug.WriteLine("Exiting");
                // was doing this in a finally block, but for some reason
                // we are hanging on response stream .Dispose so never get there
                decompressor.Dispose();
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
            }
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

        private bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                       bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }

        private void Play()
        {
            var waveOut1 = new WaveOut();
            waveOut1.Init(bufferedWaveProvider);
            waveOut1.Play();
            //Debug.WriteLine(String.Format("Started playing, waveOut.PlaybackState={0}", waveOut.PlaybackState));
            playbackState = StreamingPlaybackState.Playing;
        }

        private IWavePlayer CreateWaveOut()
        {
            return new WaveOut();
        }
    }
}