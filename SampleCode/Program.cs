using System;
using spitalkDLL;

namespace SampleCode
{
    class Program
    {
        static void Main(string[] args)
        {
            SapiTalk sapi = new SapiTalk();

            foreach(var item in sapi.Talkers())
            {
                Console.WriteLine("{0}, {1}",item.Key, item.Value);
            }

            foreach (var item in sapi.OutputDevices())
            {
                Console.WriteLine("{0}, {1}", item.Key, item.Value);
            }

            sapi.SetTalker(0);
            sapi.Talk("これはテストの音声です");

            sapi.SetTalker(5);
            sapi.Talk("これもテストの音声です");
        }
    }
}
