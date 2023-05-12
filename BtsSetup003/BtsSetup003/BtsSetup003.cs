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
    public class BtsSetup003 : Robot
    {
        [Parameter("EMA Period", DefaultValue = 9, MinValue = 2, Step = 1)]
        public int EmaPeriod { get; set; }

        private DataSeries EmaSource;
        private ExponentialMovingAverage Ema;

        protected override void OnStart()
        {
            Ema = Indicators.ExponentialMovingAverage(EmaSource, EmaPeriod);
        }

        protected override void OnBar()
        {
            base.OnBar();

            if (Positions.Count > 0)
            {
                switch (CurrentTrend)
                {
                    case Trending.Up:
                        if (LastCloseBelowEma())
                        {
                            double lastCandleLow = Bars.LowPrices.Last(1);
                            UpdateStopLoss(lastCandleLow);
                        }
                        break;
                    case Trending.Down:
                        if (LastClosedAboveEma())
                        {
                            double lastCandleHigh = Bars.HighPrices.Last(1);
                            UpdateStopLoss(lastCandleHigh);
                        }
                        break;
                }
            }
            if (Positions.Count == 0)
            {

                if (EmaTurnedUp())
                {
                    CurrentTrend = CurrentTrend.TrendingUp;
                    RefCandleHigh = Bars.HighPrices.Last(1);
                    RefCandleLow = Bars.LowPrices.Last(1);
                    StopLossPips = ((RefCandleHigh - RefCandleLow) / Symbol.PipSize) - 1;
                }
                if (EmaTurnedDown())
                {
                    CurrentTrend = CurrentTrend.TrendingDown;
                    RefCandleHigh = Bars.HighPrices.Last(1);
                    RefCandleLow = Bars.LowPrices.Last(1);
                    StopLossPips = ((RefCandleHigh - RefCandleLow) / Symbol.PipSize) + 1;
                }
            }
        }

        protected override void OnTick()
        {
            switch (CurrentTrend)
            {
                case Trending.Up:
                    if (Ask > RefCandleHigh)
                    {
                        CloseOppositePosition(TradeType.Sell);
                        EnterAtMarket(TradeType.Buy);
                    }
                    break;
                case Trending.Down:
                    if (Bid < RefCandleLow)
                    {
                        CloseOppositePosition(TradeType.Buy);
                        EnterAtMarket(TradeType.Sell);

                    }
                    break;
            }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}