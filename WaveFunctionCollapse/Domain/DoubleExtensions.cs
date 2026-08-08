namespace WaveFunctionCollapse.Domain;

public static class DoubleExtensions
{
    public static bool IsNearlyEqual(this double a, double b, double tolerance = 1e-9)
    {
        if (a == b) return true;

        return Math.Abs(a - b) <= tolerance;
    }
}
