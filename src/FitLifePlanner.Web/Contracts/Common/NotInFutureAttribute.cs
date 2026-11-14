using System.ComponentModel.DataAnnotations;

namespace FitLifePlanner.Web.Contracts.Common;

public sealed class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute() : base("The {0} field cannot be in the future.")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is not DateTime date || date <= DateTime.UtcNow;
    }
}
