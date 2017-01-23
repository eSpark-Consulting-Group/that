public class HddInfo
{
    public HddInfo(string model, string deviceId, string serialNumber
        , string signature, string totalTracks)
    {
        Model = model;
        DeviceId = deviceId;
        SerialNumber = serialNumber;
        Signature = signature;
        TotalTracks = totalTracks;
    }

    public string Model { get; }
    public string DeviceId { get; }
    public string SerialNumber { get; }
    public string Signature { get; }
    public string TotalTracks { get; }
}