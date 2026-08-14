using System;

public static class TaxCalculator
{
    public static double CalculateAccruedTax(
        int productionPerMinute,
        double elapsedSeconds
    )
    {
        if (productionPerMinute <= 0 || elapsedSeconds <= 0d)
        {
            return 0d;
        }

        return productionPerMinute * elapsedSeconds / 60d;
    }

    public static double GetEligibleSeconds(
        double elapsedSeconds,
        double alreadyUsedSeconds,
        double maximumSeconds
    )
    {
        if (elapsedSeconds <= 0d || maximumSeconds <= 0d)
        {
            return 0d;
        }

        double remainingSeconds = Math.Max(
            0d,
            maximumSeconds - alreadyUsedSeconds
        );

        return Math.Min(elapsedSeconds, remainingSeconds);
    }
}
