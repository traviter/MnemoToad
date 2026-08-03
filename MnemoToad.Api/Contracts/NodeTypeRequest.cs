using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Contracts;

public record NodeTypeRequest([Required] string Name, string? Description);
