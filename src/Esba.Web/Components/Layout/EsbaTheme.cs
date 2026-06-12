using MudBlazor;

namespace Esba.Web.Components.Layout;

/// <summary>
/// Tema institucional único de la aplicación (migration_improvements.md §3.2:
/// sucesor de los skins VCL .vsf — un solo lugar define colores y tipografía).
/// Paleta clara y oscura con azul institucional y acento verde azulado;
/// contrastes AA sobre sus fondos respectivos (§3.4).
/// </summary>
public static class EsbaTheme
{
    public static readonly MudTheme Tema = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1E40AF",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#0F766E",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#475569",
            Info = "#0369A1",
            Success = "#15803D",
            Warning = "#B45309",
            Error = "#B91C1C",
            Background = "#F1F5F9",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#0F172A",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#334155",
            DrawerIcon = "#475569",
            TextPrimary = "#0F172A",
            TextSecondary = "#475569",
            LinesDefault = "#E2E8F0",
            LinesInputs = "#CBD5E1",
            TableLines = "#E2E8F0",
            Divider = "#E2E8F0",
            ActionDefault = "#475569",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#60A5FA",
            PrimaryContrastText = "#0B1B3A",
            Secondary = "#2DD4BF",
            SecondaryContrastText = "#042F2E",
            Tertiary = "#94A3B8",
            Info = "#38BDF8",
            Success = "#4ADE80",
            Warning = "#FBBF24",
            Error = "#F87171",
            Background = "#0F172A",
            Surface = "#1E293B",
            AppbarBackground = "#0F172A",
            AppbarText = "#F1F5F9",
            DrawerBackground = "#0F172A",
            DrawerText = "#CBD5E1",
            DrawerIcon = "#94A3B8",
            TextPrimary = "#F1F5F9",
            TextSecondary = "#94A3B8",
            LinesDefault = "#334155",
            LinesInputs = "#475569",
            TableLines = "#334155",
            Divider = "#334155",
            ActionDefault = "#94A3B8",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "280px",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Roboto", "Helvetica", "Arial", "sans-serif"],
                LineHeight = "1.5",
            },
            H5 = new H5Typography { FontWeight = "600" },
            H6 = new H6Typography { FontWeight = "600" },
            Button = new ButtonTypography { TextTransform = "none", FontWeight = "600" },
        },
    };
}
