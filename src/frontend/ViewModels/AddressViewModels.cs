namespace frontend.ViewModels;

public sealed class AddAddressViewModel
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string? Apartment { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
