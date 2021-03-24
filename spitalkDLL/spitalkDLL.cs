using SpeechLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace spitalkDLL
{
    public class SapiTalk
    {
        private SpVoice sapi = null;
        private Dictionary<int, SpObjectToken> SpeakerList = new Dictionary<int, SpObjectToken>();
        private Dictionary<int, SpObjectToken> OutputDeviceList = new Dictionary<int, SpObjectToken>();

        private int Speed = 0;
        private int Volume = 100;
        private int AvatorIdx = 0;
        private int DevIdx = 0;

        public SapiTalk()
        {
            try
            {
                sapi = new SpVoice();
                SpObjectTokenCategory sapiCat = new SpObjectTokenCategory();
                Dictionary<string, SpObjectToken> TokerPool = new Dictionary<string, SpObjectToken>();

                // See https://qiita.com/7shi/items/7781516d6746e29c03b4
                sapiCat.SetId(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices", false);

                foreach (SpObjectToken token in sapiCat.EnumerateTokens())
                {
                    if (!TokerPool.ContainsKey(token.GetAttribute("name")))
                    {
                        TokerPool.Add(token.GetAttribute("name"), token);
                    }
                }

                foreach (SpObjectToken token in sapi.GetVoices("", ""))
                {
                    if (!TokerPool.ContainsKey(token.GetAttribute("name")))
                    {
                        TokerPool.Add(token.GetAttribute("name"), token);
                    }
                }

                SpeakerList = TokerPool.Select((val, idx) => new { Key = idx, Value = val.Value }).ToDictionary(s => s.Key, s => s.Value);

                ScanOutputDevices();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0},{1},{2}", e.Message, e.InnerException == null ? "" : e.InnerException.Message, e.StackTrace);
            }
        }

        public void ScanOutputDevices()
        {
            Dictionary<string, SpObjectToken> OutDevPool = new Dictionary<string, SpObjectToken>();

            int idx = 0;

            foreach (SpObjectToken token in sapi.GetAudioOutputs("", ""))
            {
                string dev = token.GetDescription();

                if (!OutDevPool.ContainsKey(dev))
                {
                    OutDevPool.Add(dev, token);
                }
            }

            OutputDeviceList.Clear();
            foreach (var item in OutDevPool)
            {
                OutputDeviceList.Add(idx, item.Value);
                idx++;
            }
        }

        public Dictionary<int,string> Talkers()
        {
            Dictionary<int, string> ans = new Dictionary<int, string>();

            for (int i = 0; i < SpeakerList.Count; i++)
            {
                ans.Add(i, SpeakerList[i].GetDescription());
            }

            return ans;
        }

        public Dictionary<int, string> OutputDevices()
        {
            Dictionary<int, string> ans = new Dictionary<int, string>();

            for (int i = 0; i < OutputDeviceList.Count; i++)
            {
                ans.Add(i, OutputDeviceList[i].GetDescription());
            }

            return ans;
        }

        public void SetTalker(int talker)
        {
            AvatorIdx = talker;
        }

        public void SetDevice(int dev)
        {
            DevIdx = dev;
        }

        public void SetVolume(int volume)
        {
            Volume = volume;
        }

        public void SetRate(int speed)
        {
            Speed = speed;
        }

        public void Talk(string text, bool asyncFlag = false)
        {
            try
            {
                SpObjectToken backupSapi = null;

                Thread t = new Thread(() =>
                {
                    backupSapi = sapi.Voice;
                    sapi.AudioOutput = OutputDeviceList[DevIdx];
                    sapi.Voice = SpeakerList[AvatorIdx];
                    sapi.Rate = Speed;
                    sapi.Volume = Volume;
                    sapi.Speak(text);
                    sapi.Voice = backupSapi;
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                if (!asyncFlag) t.Join();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("発声処理で落ちたっす。{0}", e.Message));
            }
        }

        public void Save(string filePath, string text)
        {
            try
            {
                SpObjectToken backupSapi = null;
                SpFileStream ss = new SpFileStream();
                ss.Open(filePath, SpeechStreamFileMode.SSFMCreateForWrite);
                sapi.AudioOutputStream = ss;

                Thread t = new Thread(() => {
                    backupSapi = sapi.Voice;
                    sapi.Voice = SpeakerList[AvatorIdx];
                    sapi.Rate = Speed;
                    sapi.Volume = Volume;
                    sapi.Speak(text);
                    sapi.Voice = backupSapi;
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                ss.Close();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("保存処理で落ちたっす。{0}", e.Message));
            }
        }

    }
}
