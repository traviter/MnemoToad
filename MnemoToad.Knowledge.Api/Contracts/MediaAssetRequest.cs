using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Contracts;

public record MediaAssetRequest([Required] string Url);
