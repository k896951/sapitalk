using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SpeechLib;

namespace sapitalk
{
    class Program
    {
        private static Dictionary<int, SpObjectToken> SpeakerList = null;
        private static int Speed = 0;
        private static int Volume = 100;
        private static int AvatorIdx = 0;
        private static string file = "";
        private static string text = @"sapi-talk ready!";

        static void Main(string[] args)
        {
            try
            {
                SpVoice sapi = new SpVoice();
                SpObjectTokenCategory sapiCat = new SpObjectTokenCategory();
                Dictionary<string, SpObjectToken> TokerPool = new Dictionary<string, SpObjectToken>();
                SpFileStream Sfs = null;

                // See https://qiita.com/7shi/items/7781516d6746e29c03b4
                sapiCat.SetId(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices", false);

                // Narrator voices
                foreach (SpObjectToken token in sapiCat.EnumerateTokens())
                {
                    if (!TokerPool.ContainsKey(token.GetAttribute("name")))
                    {
                        TokerPool.Add(token.GetAttribute("name"), token);
                    }
                }

                // SAPI voices
                foreach (SpObjectToken token in sapi.GetVoices("", ""))
                {
                    if (!TokerPool.ContainsKey(token.GetAttribute("name")))
                    {
                        TokerPool.Add(token.GetAttribute("name"), token);
                    }
                }

                SpeakerList = TokerPool.Select((val, idx) => new { Key = idx, Value = val.Value }).ToDictionary(s => s.Key, s => s.Value);

                if (!Opt(args)) return;

                if (file != "")
                {
                    Sfs = new SpFileStream();
                    Sfs.Open(file, SpeechStreamFileMode.SSFMCreateForWrite);
                    sapi.AudioOutputStream = Sfs;
                }

                Thread t = new Thread(() => {
                    SpObjectToken backupSapi = sapi.Voice;
                    sapi.Voice = SpeakerList[AvatorIdx];
                    sapi.Rate = Speed;
                    sapi.Volume = Volume;
                    sapi.Speak(text);
                    sapi.Voice = backupSapi;
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

                if (file != "")
                {
                    Sfs.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("err:{0},{1}", e.Message, e.InnerException == null ? "" : e.InnerException.Message);
            }
        }

        private static bool Opt(string[] args)
        {
            StringBuilder sb = new StringBuilder();

            if (args.Length == 0 )
            {
                Help();
                return false;
            }

            for (int idx = 0; idx < args.Length; idx++)
            {
                switch (args[idx])
                {
                    case "-h":
                        Help();
                        return false;

                    case "-l":
                        for (int i = 0; i < SpeakerList.Count; i++)
                        {
                            Console.WriteLine("話者番号 {0} : {1}", i, SpeakerList[i].GetDescription());
                        }
                        return false;

                    case "-t":
                        if ((idx + 1) < args.Length)
                        {
                            AvatorIdx = int.Parse(args[idx + 1]);
                            if ((AvatorIdx < 0) || (AvatorIdx >= SpeakerList.Count)) AvatorIdx = 0;
                            idx++;
                        };
                        break;

                    case "-v":
                        if ((idx + 1) < args.Length)
                        {
                            Volume = int.Parse(args[idx + 1]);
                            if (Volume < 0) Volume = 0;
                            if (Volume > 100) Volume = 100;
                            idx++;
                        };
                        break;

                    case "-s":
                        if ((idx + 1) < args.Length)
                        {
                            Speed = int.Parse(args[idx + 1]);
                            if (Speed < -10) Speed = -10;
                            if (Speed > 10) Speed = 10;
                            idx++;
                        }
                        break;

                    case "-f":
                        if ((idx + 1) < args.Length)
                        {
                            file = args[idx + 1];
                            idx++;
                        }
                        break;

                    default:
                        sb.Append(args[idx]);
                        break;
                }
            }

            if (sb.Length != 0) text = sb.ToString();

            return true;
        }

        private static void Help()
        {
            Console.WriteLine("usage: sapitalk [-l]");
            Console.WriteLine("       sapitalk [-t 話者番号] [-v 音量] [-s 話速] [-f 保存ファイル名] 読みあげるテキスト");
            Console.WriteLine("       -l 利用可能話者一覧の出力");
            Console.WriteLine("       -t 話者番号。指定無しは0");
            Console.WriteLine("       -s -10 ～  10 の間で指定無しは  0");
            Console.WriteLine("       -v   0 ～ 100 の間で指定無しは100");
        }
    }
}
