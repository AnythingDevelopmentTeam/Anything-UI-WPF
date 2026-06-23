using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Anything_UI_WPF.Native;

internal static class AppIcons
{
    public static FontIcon CreateIcon(string glyph, double size = 18)
    {
        return new FontIcon
        {
            Glyph = glyph,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = size,
        };
    }

    public const string Settings = "\uE713";
    public const string Info = "\uE946";
    public const string Sun = "\uE706";
    public const string Moon = "\uE708";
    public const string Search = "\uE721";
    public const string Refresh = "\uE72C";
}