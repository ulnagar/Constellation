using System;

namespace Constellation.Application.Helpers
{
    public static class BandwidthDisplayHelper
    {
        public static string DecimalToRoundedString(decimal? value)
        {
            if (value == null)
                return "N/A";

            return $"{Decimal.Round(value.Value, 2)} Mbps";
        }

        public static string UsageToPercentString(int? bandwidth, decimal? value)
        {
            if (bandwidth == null || value == null)
                return "N/A";

            var bwInMb = (bandwidth / 1000000);
            Decimal percent = (decimal)((value.Value / bwInMb) * 100);

            return $"{Math.Round(percent, 2).ToString("0.##")}%";
        }

        public static string DecimalToFormattedString(decimal? bandwidth)
        {
            if (bandwidth == null)
            {
                return "N/A";
            }

            var bwInKb = (bandwidth.Value / 1000);

            if (bwInKb < 1)
            {
                return $"{Math.Round(bandwidth.Value, 2).ToString("0.##")} bps";
            }

            var bwInMb = (bwInKb / 1000);

            if (bwInMb < 1)
            {
                return $"{Math.Round(bwInKb, 2).ToString("0.##")} Kbps";
            }

            var bwInGb = (bwInMb / 1000);

            if (bwInGb < 1)
            {
                return $"{Math.Round(bwInMb, 2).ToString("0.##")} Mbps";
            }
            else
            {
                return $"{Math.Round(bwInGb, 2).ToString("0.##")} Gbps";
            }
        }
    }
}
