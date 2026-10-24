using System.ComponentModel.DataAnnotations;

namespace InvoiceKeeper.Application.Validation;

/// <summary>Атрибут валидации: значение должно входить в указанный набор.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class AllowedValuesAttribute : ValidationAttribute
{
    private readonly string[] _values;

    public AllowedValuesAttribute(params string[] values)
    {
        _values = values;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var str = value.ToString();
        if (str is not null && _values.Contains(str, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Success;
        }

        var allowed = string.Join(", ", _values);
        return new ValidationResult($"Поле {validationContext.DisplayName} должно быть одним из: {allowed}.");
    }
}
