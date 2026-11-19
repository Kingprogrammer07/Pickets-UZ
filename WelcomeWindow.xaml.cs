using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pickets;

public partial class WelcomeWindow : AccessibleDialogWindow
{
    private static readonly Brush ReadyBrush = new SolidColorBrush(Color.FromRgb(0x63, 0xC1, 0x78));
    private static readonly Brush ActionBrush = new SolidColorBrush(Color.FromRgb(0xF0, 0xB3, 0x5A));

    public bool CreateDesktopShortcut => CreateShortcutCheck.IsChecked == true && CreateShortcutCheck.IsEnabled;
    public bool RunAtLogin => RunAtLoginCheck.IsChecked == true;
    private readonly Func<DesktopReadiness> _readReadiness;

    public WelcomeWindow(bool shortcutExists, bool runAtLogin)
        : this(shortcutExists, runAtLogin, DesktopReadiness.Read) { }

    internal WelcomeWindow(bool shortcutExists, bool runAtLogin, Func<DesktopReadiness> readReadiness)
    {
        _readReadiness = readReadiness;
        InitializeComponent();
        CreateShortcutCheck.IsChecked = OnboardingPreferences.DefaultCreateShortcut(shortcutExists);
        if (shortcutExists)
        {
            CreateShortcutCheck.Content = new TextBlock
            {
                Text = "Pickets ish stoli yorlig‘i tayyor", TextWrapping = TextWrapping.Wrap,
            };
            CreateShortcutCheck.IsEnabled = false;
        }
        RunAtLoginCheck.IsChecked = runAtLogin;
        RefreshDesktopStatus();
        Activated += (_, _) => RefreshDesktopStatus();
    }

    private void Recheck_Click(object sender, RoutedEventArgs e) => RefreshDesktopStatus();

    private DesktopReadiness RefreshDesktopStatus()
    {
        var readiness = _readReadiness();
        GetStartedButton.IsEnabled = readiness.IsReady;
        GetStartedButton.ToolTip = readiness.IsReady ? "Ish stolingiz tayyor." : "Davom etish uchun yuqoridagi ish stoli sozlamalarini bajaring.";
        SetStatus(AutoArrangeStatus, AutoArrangeIndicator, "Belgilarni avtomatik tartiblash", readiness.AutoArrange);
        SetStatus(SnapToGridStatus, SnapToGridIndicator, "Belgilarni to‘rga tekislash", readiness.AlignToGrid);
        ReadinessHint.Text = readiness.IsReady ? "Ish stolingiz tayyor."
            : readiness.IsUnknown ? "Windows Explorer’ni tekshirib bo‘lmadi. Ish stoli yuklanishini kuting va «Qayta tekshirish»ni bosing."
            : "Boshlashdan oldin Ish stoli → Ko‘rinish menyusidagi ikkala sozlamani o‘chiring. So‘ng «Qayta tekshirish»ni bosing.";
        return readiness;
    }

    private static void SetStatus(TextBlock text, Ellipse indicator,
                                  string label, bool? enabled)
    {
        text.Text = enabled == null ? $"{label}: tekshirib bo‘lmadi"
            : enabled == true ? $"{label}: o‘chiring" : $"{label}: tayyor";
        indicator.Fill = enabled == false ? ReadyBrush : ActionBrush;
    }

    private void GetStarted_Click(object sender, RoutedEventArgs e)
    {
        if (!RefreshDesktopStatus().IsReady)
        {
            ReadinessHint.BringIntoView();
            return;
        }
        DialogResult = true;
        Close();
    }

}
