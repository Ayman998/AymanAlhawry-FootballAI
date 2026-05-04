namespace FootballAI.Application.Helpers;

public static class FieldScaleComputer
{
    public static double ComputeFieldScale(int videoWidthPixels)
    {
        const double pitchWidthMeters = 105.0;
        return pitchWidthMeters / videoWidthPixels;
    }
}
