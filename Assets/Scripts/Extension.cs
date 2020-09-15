using Unity.Mathematics;

public static class Extension
{
    public static int Amount(this int3 x)
        => x[0] * x[1] * x[2];
}
    