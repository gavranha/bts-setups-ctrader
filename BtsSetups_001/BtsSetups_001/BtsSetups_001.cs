using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class BtsSetups_001 : Robot
    {
        [Parameter("Slow EMA period", DefaultValue = 50, MinValue = 2, Step = 1)]
        public int SlowEmaPeriod { get; set; }
        [Parameter("Fast EMA period", DefaultValue = 9, MinValue = 2, Step = 1)]
        public int FastEmaPeriod { get; set; }

        private DataSeries EmaSource;
        private ExponentialMovingAverage SlowEma;
        private ExponentialMovingAverage FastEma;

        protected override void OnStart()
        {
            SlowEma = Indicators.ExponentialMovingAverage(EmaSource, SlowEmaPeriod);
            FastEma = Indicators.ExponentialMovingAverage(EmaSource, FastEmaPeriod);
        }

        protected override void OnBar()
        {
            base.OnBar();

            if (HasBuySignal())
            {
                Print($"HasBuySignal {HasBuySignal()}");
            }
            else if (HasSellSignal())
            {
                Print($"HasSellSignal {HasSellSignal()}");
            }
        }

        private bool HasSellSignal()
        {
            return FastEma.Result.Last(2) >= SlowEma.Result.Last(2) && FastEma.Result.Last(1) < SlowEma.Result.Last(1);
        }

        private bool HasBuySignal()
        {
            return FastEma.Result.Last(2) <= SlowEma.Result.Last(2) && FastEma.Result.Last(1) > SlowEma.Result.Last(1);
        }
    }
}