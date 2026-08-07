namespace MnemoToad.Knowledge.Data.Entities;

public class MediaAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
}
