using System;

namespace TitanReach_Server.Utilities
{
    public class ST_Timer
    {

        public int TimeStarted = Environment.TickCount;
        public int Delay;
        //    public Action<ST_Timer> Action;
        public Action<ST_Timer, Object[]> Action;
        public Action FinishedCallback;
        public Object[] param;
        public bool Repeat = false;
        public int LoopCount = 0;

        public void Stop()
        {
            LoopCount = 0;
            Delay = 0;
            Repeat = false;
            Server.Instance.timers.Remove(this);
        }
    }
}
