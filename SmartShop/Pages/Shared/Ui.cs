using System.Globalization;

namespace SmartShop.Pages.Shared;

public static class Ui
{
    private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");

    public static string CategoryClass(string category) => category.ToLowerInvariant() switch
    {
        "phone" => "cat-phone",
        "laptop" => "cat-laptop",
        "accessory" => "cat-accessory",
        _ => "cat-other",
    };

    public static string Vnd(decimal price) => price.ToString("N0", Vietnamese) + " ₫";

    public static string OneDecimal(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

    public static string Percent(double part, double total) =>
        (total <= 0 ? 0 : part / total * 100).ToString("0.##", CultureInfo.InvariantCulture);

    public static IEnumerable<string> SplitFeatures(string features) =>
        features.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
