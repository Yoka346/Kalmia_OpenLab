using System;
using System.Threading.Tasks;

namespace Kalmia_OpenLab.View
{
    internal class Animator
    {
        public event EventHandler OnEndAnimation = delegate { };

        Func<int, int, bool> callback;
        bool stopFlag;

        public Animator(Func<int, int, bool> callback) => this.callback = callback;

        public void AnimateFornumFrames(double interval, int numFrames)
        {
            this.stopFlag = false;

            Task.Run(() =>
            {
                var nextTiming = (double)Environment.TickCount;
                for (var frameCount = 0; !this.stopFlag && frameCount < numFrames; frameCount++)
                {
                    while (Environment.TickCount < nextTiming) ;
                    if (!this.callback(frameCount, numFrames))
                        break;
                    nextTiming += interval;
                }
                this.OnEndAnimation.Invoke(this, EventArgs.Empty);
            });
        }

        public void AnimateForDuration(double interval, int duration)
        {
            if (duration < interval)
                duration = (int)interval;
            AnimateFornumFrames(interval, (int)(duration / interval));
        }

        public void Stop() => this.stopFlag = true;
    }
}
