using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Contracts;

public record AttributeTypeRequest([Required] string Name, string? Description);
