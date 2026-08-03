using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Contracts;

public sealed class RequiredGuidAttribute : ValidationAttribute
{
    public RequiredGuidAttribute() : base("The {0} field is required.") { }

    public override bool IsValid(object? value) => value is Guid guid && guid != Guid.Empty;
}
