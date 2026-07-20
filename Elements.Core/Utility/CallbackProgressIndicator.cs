using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class CallbackProgressIndicator : IProgressIndicator
    {
        ProgressUpdate update;
        ProgressFinish finish;
        ProgressFinish fail;

        public CallbackProgressIndicator(ProgressUpdate update, ProgressFinish finish, ProgressFinish fail)
        {
            this.update = update;
            this.finish = finish;
            this.fail = fail;
        }

        public void ProgressDone(LocaleString message)
        {
            finish?.Invoke(message);
        }

        public void ProgressFail(LocaleString message)
        {
            fail?.Invoke(message);
        }

        public void UpdateProgress(float percent, LocaleString progressInfo, LocaleString detailInfo)
        {
            update?.Invoke(percent, progressInfo, detailInfo);
        }
    }
}
