using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public delegate void ProgressUpdate(float percent, LocaleString progressInfo, LocaleString detailInfo);
    public delegate void ProgressFinish(LocaleString message);

    public interface IProgressIndicator
    {
        void UpdateProgress(float percent, LocaleString progressInfo, LocaleString detailInfo);
        void ProgressDone(LocaleString message);
        void ProgressFail(LocaleString message);
    }

    public class ProgressIndicatorWrapper : IProgressIndicator
    {
        public float ProgressFrom { get; private set; }
        public float ProgressTo { get; private set; }

        public IProgressIndicator Inner { get; private set; }

        public ProgressIndicatorWrapper(float progressFrom, float progressTo, IProgressIndicator inner)
        {
            ProgressFrom = progressFrom;
            ProgressTo = progressTo;
            Inner = inner;
        }

        public void UpdateProgress(float percent, LocaleString progressInfo, LocaleString detailInfo)
        {
            // Remap the progress to the new range
            if (percent >= 0f)
                percent = MathX.Lerp(ProgressFrom, ProgressTo, percent);

            Inner.UpdateProgress(percent, progressInfo, detailInfo);
        }

        public void ProgressDone(LocaleString message) => Inner.ProgressDone(message);
        public void ProgressFail(LocaleString message) => Inner.ProgressFail(message);
    }

    public static class ProgressIndicatorHelper
    {
        public static ProgressIndicatorWrapper Remap(this IProgressIndicator indicator, float from, float to)
        {
            return new ProgressIndicatorWrapper(from, to, indicator);
        }
    }
}
