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
        private Position Position;
        [Parameter("Quantity (lots)", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        private DataSeries EmaSource;
        private ExponentialMovingAverage Ema;
        private Trending CurrentTrend;
        private double StopLossPips;
        private double RefCandleHigh;
        private double RefCandleLow;
        private double VolumeInUnits { get => Symbol.QuantityToVolumeInUnits(Quantity); }
        private const string Label = "BtsSetup003";

        protected override void OnStart()
        {
            Ema = Indicators.ExponentialMovingAverage(EmaSource, EmaPeriod);
            CurrentTrend = GetCurrentTrend();
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
                RefCandleHigh = Bars.HighPrices.Last(1);
                RefCandleLow = Bars.LowPrices.Last(1);

                if (EmaTurnedUp())
                {
                    CurrentTrend = Trending.Up;
                    StopLossPips = ((RefCandleHigh - RefCandleLow) / Symbol.PipSize) - 1;
                }
                if (EmaTurnedDown())
                {
                    CurrentTrend = Trending.Down;
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
                        CloseOppositePosition(Position);
                        EnterAtMarket(TradeType.Buy);
                    }
                    break;

                case Trending.Down:
                    if (Bid < RefCandleLow)
                    {
                        CloseOppositePosition(Position);
                        EnterAtMarket(TradeType.Sell);

                    }
                    break;
            }
        }

        private bool EmaTurnedDown()
        {
            return Ema.Result.IsFalling() ? true : false;
            // double EmaPoint1 = Ema.Result.Last(1);
            // double EmaPoint2 = Ema.Result.Last(2);
            // double EmaPoint3 = Ema.Result.Last(3);
            // double EmaPoint4 = Ema.Result.Last(4);
            // double EmaPoint5 = Ema.Result.Last(5);
            // if (EmaPoint1 < EmaPoint2 && EmaPoint2 < EmaPoint3
            //     && EmaPoint3 > EmaPoint4 && EmaPoint4 > EmaPoint5)
            // {
            //     return true;
            // }
            // return false;
        }

        private bool EmaTurnedUp()
        {
            return Ema.Result.IsRising() ? true : false;
            // double EmaPoint1 = Ema.Result.Last(1);
            // double EmaPoint2 = Ema.Result.Last(2);
            // double EmaPoint3 = Ema.Result.Last(3);
            // double EmaPoint4 = Ema.Result.Last(4);
            // double EmaPoint5 = Ema.Result.Last(5);
            // if (EmaPoint1 > EmaPoint2 && EmaPoint2 > EmaPoint3
            //     && EmaPoint3 < EmaPoint4 && EmaPoint4 < EmaPoint5)
            // {
            //     return true;
            // }
            // return false;
        }

        private void UpdateStopLoss(double newStopLoss)
        {
            Position position = Positions.Find(Label);
            TradeResult tradeResult = ModifyPosition(position, newStopLoss, null);
            if (!tradeResult.IsSuccessful)
            {
                Print($"Failed to update stop-loss: {tradeResult.Error}");
            }
        }

        private bool LastClosedAboveEma()
        {
            return Bars.ClosePrices.Last(1) > Ema.Result.Last(1) ? true : false;
        }

        private bool LastCloseBelowEma()
        {
            return Bars.ClosePrices.Last(1) < Ema.Result.Last(1) ? true : false;
        }

        private void EnterAtMarket(TradeType tradeType)
        {
            TradeResult tradeResult = ExecuteMarketOrder(
                tradeType, SymbolName, VolumeInUnits, Label, StopLossPips, null);
            if (!tradeResult.IsSuccessful)
            {
                Print($"Failed to execute market order: {tradeResult.Error}");
                Stop();
            }
            Position = tradeResult.Position;

        }

        private void CloseOppositePosition(Position position)
        {
            position = Positions.Find(Label);
            TradeResult tradeResult = ClosePosition(position);
            if (!tradeResult.IsSuccessful)
            {
                Print($"Failed to close opposite position: {tradeResult.Error}");
            }
        }

        private Trending GetCurrentTrend()
        {
            if (Ema.Result.IsRising()) { return Trending.Up; }
            if (Ema.Result.IsFalling()) { return Trending.Down; }
            else { return Trending.Flat; }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}