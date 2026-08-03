using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Contracts;

public record NodeTypeRequest([Required] string Name, string? Description);
