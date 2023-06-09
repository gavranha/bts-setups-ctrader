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
    public class BtsSetup001 : Robot
    {
        [Parameter("Slow EMA period", Group = "Signal", DefaultValue = 50, MinValue = 2, Step = 1)]
        public int SlowEmaPeriod { get; set; }
        [Parameter("Fast EMA period", Group = "Signal", DefaultValue = 9, MinValue = 2, Step = 1)]
        public int FastEmaPeriod { get; set; }

        [Parameter("Quantity", Group = "Trading", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        private DataSeries EmaSource;
        private ExponentialMovingAverage SlowEma;
        private ExponentialMovingAverage FastEma;
        private double VolumeInUnits { get => Symbol.QuantityToVolumeInUnits(Quantity); }
        private const string Label = "BtsSetup001";

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

                foreach (Position position in Positions.FindAll(Label))
                {
                    ClosePosition(position);
                }

                EnterAtMarket(TradeType.Buy);
            }
            else if (HasSellSignal())
            {
                Print($"HasSellSignal {HasSellSignal()}");

                foreach (Position position in Positions.FindAll(Label))
                {
                    ClosePosition(position);
                }

                EnterAtMarket(TradeType.Sell);
            }
        }

        private void EnterAtMarket(TradeType tradeType)
        {
            TradeResult tradeResult = ExecuteMarketOrder(
                tradeType, SymbolName, VolumeInUnits, Label, null, null);

            if (!tradeResult.IsSuccessful)
            {
                Print("Market Order failed: {0}", tradeResult.Error);
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