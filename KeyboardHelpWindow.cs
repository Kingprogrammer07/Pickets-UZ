using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pickets;

public sealed class KeyboardHelpWindow : AccessibleDialogWindow
{
    public KeyboardHelpWindow(string shortcut)
    {
        Title = "Pickets: klaviatura bo‘yicha yordam";
        Width = 560;
        Height = 620;
        ResizeMode = ResizeMode.CanResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var root = new DockPanel { Margin = new Thickness(20) };
        var close = new Button { Content = "Yopish", IsCancel = true, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,12,0,0) };
        close.Click += (_, _) => Close();
        DockPanel.SetDock(close, Dock.Bottom);
        root.Children.Add(close);
        var content = new StackPanel();
        content.Children.Add(new TextBlock { Text = "Klaviatura bo‘yicha yordam", FontSize = 24, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        content.Children.Add(new TextBlock { Text = shortcut == "None" ? "Umumiy tugmalar o‘chirilgan. Tizim panelidagi «Pickets’ni faollashtirish» bandidan foydalaning." :
            "Pickets’ni faollashtirish: " + shortcut + ". Faol holatda yana bossangiz yashiriladi.",
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,16) });
        var entries = new (string Key, string Action)[]
        {
            ("Ctrl+Tab / Ctrl+Shift+Tab", "Keyingi yoki oldingi guruhga, shu jumladan to‘plamning boshqa sahifalariga o‘tish"),
            ("Tab / Shift+Tab", "Boshqaruv elementlari orasida o‘tish"),
            ("Sarlavhada Enter yoki Space", "Guruhni ochish yoki yig‘ish"),
            ("Sarlavhada pastga o‘q", "Guruhni ochib, ichidagi havolalarga o‘tish"),
            ("O‘q tugmalari", "Havolalar orasida yurish"),
            ("Space / Ctrl+Space", "Tanlash yoki tanlovni almashtirish"),
            ("Shift+O‘q", "Tanlovni kengaytirish"),
            ("Ctrl+A", "Ushbu guruhdagi barcha havolalarni tanlash"),
            ("Ctrl+Z", "Oxirgi amalni bekor qilish; sarlavha va tizim paneli menyusida ham bor"),
            ("Ctrl+F", "Yig‘ilgan guruhlarni ham qo‘shib, barcha havolalarni qidirish"),
            ("Bo‘sh joydan sudrash", "To‘rtburchak bilan tanlash. Shift — qo‘shish, Ctrl — tanlovni almashtirish, Escape — bekor qilish. Aylantirish uchun chetga yaqin sudrang."),
            ("Tanlangan elementni sudrash", "Tanlanganlarning barchasini boshqa guruhga ko‘chirish; ko‘chadigan elementlar soni ko‘rsatiladi"),
            ("Havolada Enter", "Tanlangan havolalarni ochish"),
            ("Delete", "Tanlangan havolalarni olib tashlash; asl fayllar o‘chirilmaydi"),
            ("Shift+F10", "«Havolani ko‘chirish» bandi bor kontekst menyusi"),
            ("F2", "Ushbu guruh nomini o‘zgartirish"),
            ("Escape", "Guruhni yig‘ib, sarlavhasini faollashtirish"),
            ("Ctrl+O / Ctrl+Shift+O", "Fayllar yoki papkalarni qo‘shish"),
            ("Ctrl+N", "Yangi guruh yaratish"),
            ("Ctrl+Shift+Up / Down", "Faol havolaning yoki sarlavhasi faol bo‘lsa, guruhning o‘rnini o‘zgartirish"),
            ("Alt+Arrow", "Butun to‘plamni ko‘chirish"),
            ("Ctrl+Alt+O‘q", "Butun to‘plam o‘lchamini o‘zgartirish"),
            ("Ctrl+PageUp / PageDown", "Katta to‘plamning oldingi yoki keyingi sahifasiga o‘tish"),
            ("Alt+F4", "Pickets’ni yashirish; qaytish uchun tizim paneli yoki faollashtirish tugmalaridan foydalaning"),
            ("F1", "Ushbu yordamni ochish")
        };
        foreach (var (key, action) in entries)
        {
            content.Children.Add(new TextBlock { Text = key, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,9,0,2) });
            content.Children.Add(new TextBlock { Text = action, TextWrapping = TextWrapping.Wrap });
        }
        var scroll = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        System.Windows.Automation.AutomationProperties.SetName(scroll, "Klaviatura tugmalari");
        root.Children.Add(scroll);
        Content = root;
        PreviewKeyDown += (_, e) => { if (e.Key == Key.Escape) { Close(); e.Handled = true; } };
    }
}
