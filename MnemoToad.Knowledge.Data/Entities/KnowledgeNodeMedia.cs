using System.Text.Json.Nodes;

namespace MnemoToad.Knowledge.Data.Entities;

public class KnowledgeNodeMedia
{
    public Guid KnowledgeNodeId { get; set; }
    public string Key { get; set; } = string.Empty;
    public Guid MediaAssetId { get; set; }
    public string AltText { get; set; } = string.Empty;
    public JsonObject? Metadata { get; set; }
}
