namespace Constellation.Application.Extensions;

using System;

public static class DecimalExtensions
{
    public static string ToRoundedString(this decimal? value)
    {
        if (value == null)
            return "N/A";

        return $"{Decimal.Round(value.Value, 2)} Mbps";
    }

    public static string ToFormattedString(this decimal bandwidth)
    {
        if (bandwidth == 0)
        {
            return "N/A";
        }

        var bwInKb = (bandwidth / 1000);

        if (bwInKb < 1)
        {
            return $"{Math.Round(bandwidth, 2).ToString("0.##")} bps";
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