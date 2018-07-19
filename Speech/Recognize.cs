
using CommandLine;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;

namespace GoogleCloudSamples
{


    public static class StringExtensions
    {
        public static bool Contains(this String str, String substring,
                                    StringComparison comp)
        {
            if (substring == null)
                throw new ArgumentNullException("substring",
                                                "substring cannot be null.");
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison",
                                            "comp");

            return str.IndexOf(substring, comp) >= 0;
        }
    }
    class Options
    {
        [Value(0, HelpText = "A path to a sound file.  Encoding must be "
            + "Linear16 with a sample rate of 16000.", Required = true)]
        public string FilePath { get; set; }
    }

    class StorageOptions
    {
        [Value(0, HelpText = "A path to a sound file. "
            + "Can be a local file path or a Google Cloud Storage path like "
            + "gs://my-bucket/my-object. "
            + "Encoding must be "
            + "Linear16 with a sample rate of 16000.", Required = true)]
        public string FilePath { get; set; }
    }

    [Verb("sync", HelpText = "Detects speech in an audio file.")]
    class SyncOptions : StorageOptions
    {
        [Option('w', HelpText = "Report the time offsets of individual words.")]
        public bool EnableWordTimeOffsets { get; set; }
    }

    [Verb("with-context", HelpText = "Detects speech in an audio file."
        + " Add additional context on stdin.")]
    class OptionsWithContext : StorageOptions { }

    [Verb("async", HelpText = "Creates a job to detect speech in an audio "
        + "file, and waits for the job to complete.")]
    class AsyncOptions : StorageOptions
    {
        [Option('w', HelpText = "Report the time offsets of individual words.")]
        public bool EnableWordTimeOffsets { get; set; }
    }

    [Verb("sync-creds", HelpText = "Detects speech in an audio file.")]
    class SyncOptionsWithCreds
    {
        [Value(0, HelpText = "A path to a sound file.  Encoding must be "
            + "Linear16 with a sample rate of 16000.", Required = true)]
        public string FilePath { get; set; }

        [Value(1, HelpText = "Path to Google credentials json file.", Required = true)]
        public string CredentialsFilePath { get; set; }
    }

    [Verb("stream", HelpText = "Detects speech in an audio file by streaming "
        + "it to the Speech API.")]
    class StreamingOptions : Options { }

    [Verb("listen", HelpText = "Detects speech in a microphone input stream.")]
    class ListenOptions
    {
        [Value(0, HelpText = "Number of seconds to listen for.", Required = false)]
        public int Seconds { get; set; } = 3;
    }

    [Verb("rec", HelpText = "Detects speech in an audio file. Supports other file formats.")]
    class RecOptions : Options
    {
        [Option('b', Default = 16000, HelpText = "Sample rate in bits per second.")]
        public int BitRate { get; set; }

        [Option('e', Default = RecognitionConfig.Types.AudioEncoding.Linear16,
            HelpText = "Audio file encoding format.")]
        public RecognitionConfig.Types.AudioEncoding Encoding { get; set; }
    }


    public class Recognize
    {
        private static ArrayList alternatives = new ArrayList();
        private static bool isReliable = false;

        public static bool IsReliable { get => isReliable; set => isReliable = value; }
        public static ArrayList Alternatives { get => alternatives; set => alternatives = value; }

        public static async Task<object> StreamingMicRecognizeAsync(int seconds, String lang)
        {
            if (NAudio.Wave.WaveIn.DeviceCount < 1)
            {
                Console.WriteLine("No microphone!");
                return -1;
            }
            ArrayList possible_words = new ArrayList();
            var speech = SpeechClient.Create();
            var streamingCall = speech.StreamingRecognize();
            // Write the initial request with the config.
            await streamingCall.WriteAsync(
                new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding =
                            RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = lang,
                        },
                        InterimResults = true,
                    }
                });
            // Print responses as they arrive.
            Task printResponses = Task.Run(async () =>
            {
                alternatives = new ArrayList();
                while (await streamingCall.ResponseStream.MoveNext(
                    default(CancellationToken)))
                {
                    foreach (var result in streamingCall.ResponseStream
                        .Current.Results)
                    {
                        foreach (var alternative in result.Alternatives)
                        {
                            alternatives.Add(alternative.Transcript);
                            if (alternative.Confidence > 0)
                            {
                                possible_words.Add(alternative.Transcript);
                                Console.WriteLine(alternative.Transcript + " Confidence: " + alternative.Confidence);
                                if (alternative.Confidence > 0.8)
                                {
                                    IsReliable = true;
                                }
                                else
                                {
                                    IsReliable = false;
                                }
                            }



                        }
                    }
                }
            });
            // Read from the microphone and stream to API.
            object writeLock = new object();
            bool writeMore = true;
            var waveIn = new NAudio.Wave.WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
            waveIn.DataAvailable +=
                (object sender, NAudio.Wave.WaveInEventArgs args) =>
                {
                    lock (writeLock)
                    {
                        if (!writeMore) return;
                        streamingCall.WriteAsync(
                            new StreamingRecognizeRequest()
                            {
                                AudioContent = Google.Protobuf.ByteString
                                    .CopyFrom(args.Buffer, 0, args.BytesRecorded)
                            }).Wait();
                    }
                };
            waveIn.StartRecording();
            Console.WriteLine("Speak now.");
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            // Stop recording and shut down.
            waveIn.StopRecording();
            lock (writeLock) writeMore = false;
            await streamingCall.WriteCompleteAsync();
            await printResponses;
            return possible_words;
        }
        // [END speech_streaming_mic_recognize]

        static bool IsStorageUri(string s) => s.Substring(0, 4).ToLower() == "gs:/";


    }
}


