using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class ConsoleProgressIndicator : IProgressIndicator
    {
        LocaleString _previousProgressInfo;

        public void ProgressDone(LocaleString message)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("DONE: " + message);

            Console.ForegroundColor = prevColor;
        }

        public void ProgressFail(LocaleString message)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("FAIL: " + message);

            Console.ForegroundColor = prevColor;
        }

        public void UpdateProgress(float percent, LocaleString progressInfo, LocaleString detailInfo)
        {
            // Erase previous percent
            Console.CursorLeft = 0;
            Console.Write(new string (' ', Console.WindowWidth - 1));
            Console.CursorLeft = 0;

            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;

            if (progressInfo != _previousProgressInfo)
                Console.WriteLine(progressInfo);

            _previousProgressInfo = progressInfo;

            if (!string.IsNullOrWhiteSpace(detailInfo.content))
                Console.WriteLine("\t" + detailInfo);

            Console.ForegroundColor = prevColor;

            // Write the percent
            Console.Write($"{percent * 100:0.00}%");
        }
    }
}
