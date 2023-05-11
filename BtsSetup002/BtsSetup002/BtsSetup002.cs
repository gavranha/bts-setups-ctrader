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
    public class BtsSetup002 : Robot
    {
        [Parameter("Quantity", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("RSI Period", DefaultValue = 2, MinValue = 1, Step = 1)]
        public int RsiPeriod { get; set; }

        [Parameter("RSI Level", DefaultValue = 10, MinValue = 1, Step = 1)]
        public double RsiLevelThreshold { get; set; }

        [Parameter("Max Open Positions", DefaultValue = 1, MinValue = 1, Step = 1)]
        public int MaxOpenPositions { get; set; }

        private double VolumeInUnits { get => Symbol.QuantityToVolumeInUnits(Quantity); }
        private RelativeStrengthIndex Rsi;
        private DataSeries RsiSource;
        private const string Label = "BtsSetup002";

        protected override void OnStart()
        {
            Rsi = Indicators.RelativeStrengthIndex(RsiSource, RsiPeriod);
        }

        protected override void OnBar()
        {
            base.OnBar();

            if (Positions.Count < MaxOpenPositions)
            {
                if (Rsi.Result.Last(1) < RsiLevelThreshold)
                {
                    OrderToBuyLastClose();
                }
            }

            if (Positions.Count > 0)
            {
                foreach (Position position in Positions.FindAll(Label))
                {
                    UpdateTakeProfit(position);

                    if (positionIsSevenDaysOlder(position))
                    {
                        ClosePosition(position);
                    }
                }
            }
        }

        private bool positionIsSevenDaysOlder(Position position)
        {
            if ((Time - position.EntryTime).TotalDays >= 7)
            {
                Print($"Closing position opened in {position.EntryTime}");
                return true;
            }
            return false;
        }

        private void UpdateTakeProfit(Position position)
        {
            double takeProfit = Math.Max(Bars.HighPrices.Last(1), Bars.HighPrices.Last(2));
            TradeResult tradeResult = ModifyPosition(position, null, takeProfit);
            if (!tradeResult.IsSuccessful)
            {
                Print($"Failed to update take profit:  {tradeResult.Error}");
            }
        }

        private void OrderToBuyLastClose()
        {
            double targetPrice = Bars.ClosePrices.Last(1);
            double takeProfitPips = GetTakeProfitInPips(targetPrice);
            TradeResult tradeResult = PlaceLimitOrder(
                TradeType.Buy, SymbolName, VolumeInUnits, targetPrice, Label, null, takeProfitPips);
            if (!tradeResult.IsSuccessful)
            {
                Print($"Failed to place limit order:  {tradeResult.Error}");
            }
        }

        private double GetTakeProfitInPips(double targetPrice)
        {
            double twoPreviousPeriodsHigh = Math.Max(Bars.HighPrices.Last(1), Bars.HighPrices.Last(2));
            return (twoPreviousPeriodsHigh - targetPrice) / Symbol.PipSize;
        }
        protected override void OnTick()
        {
            // Handle price updates here
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}