using System;
using System.IO;
using System.Runtime.InteropServices;
using PortAudioSharp;
using Vosk;


namespace MicrophoneVosk
{
    class Program
    {
        static StreamParameters oParams;
        static Model model = new Model("model");
        static VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
        static void Main(string[] args)
        {
            PortAudio.LoadNativeLibrary();
            PortAudio.Initialize();

            oParams.device = PortAudio.DefaultInputDevice;
            if (oParams.device == PortAudio.NoDevice)
                throw new Exception("No default audio input device available");

            oParams.channelCount = 1;
            oParams.sampleFormat = SampleFormat.Int16;
            oParams.hostApiSpecificStreamInfo = IntPtr.Zero;

            var callbackData = new VoskCallbackData()
                {
                    textResult=String.Empty
                };

            var stream = new PortAudioSharp.Stream(
                oParams,
                null,
                16000,
                8192,
                StreamFlags.ClipOff,
                playCallback,
                callbackData
            );

            stream.Start();
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            stream.Stop();
        }

        class VoskCallbackData
        {
            public String textResult { get; set; }
        }

        private static StreamCallbackResult playCallback(
            IntPtr input, IntPtr output,
            System.UInt32 frameCount,
            ref StreamCallbackTimeInfo timeInfo,
            StreamCallbackFlags statusFlags,
            IntPtr dataPtr
        )
        {
            byte[] buffer = new byte[frameCount];
            Marshal.Copy(input , buffer, 0, buffer.Length);
            System.IO.Stream streamInput = new MemoryStream(buffer);
            using(System.IO.Stream source = streamInput) {
                byte[] bufferRead = new byte[frameCount];
                int bytesRead;
                while((bytesRead = source.Read(bufferRead, 0, bufferRead.Length)) > 0) {
                    if (rec.AcceptWaveform(bufferRead, bytesRead)) {
                        Console.WriteLine(rec.Result());
                    }
                }
            }

            return StreamCallbackResult.Continue;
            
        }
    }
}
