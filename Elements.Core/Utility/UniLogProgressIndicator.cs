using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class UniLogProgressIndicator : IProgressIndicator
    {
        public void ProgressDone(LocaleString message) => UniLog.Log("DONE: " + message);

        public void ProgressFail(LocaleString message) => UniLog.Warning("FAIL: " + message);

        public void UpdateProgress(float percent, LocaleString progressInfo, LocaleString detailInfo) =>
            UniLog.Log($"[{percent * 100:F2}%] {progressInfo} - {detailInfo}");
    }
}
