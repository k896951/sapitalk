using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Reflection;

namespace SpeechPlatformSample
{
    class Program
    {
        static void Main(string[] args)
        {
            SpeechSynthesizer sp = new SpeechSynthesizer();

            var ti = sp.GetType();

            var mi1 = ti.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            var mi2 = sp.GetType().GetMethod("SetOutputStream", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
