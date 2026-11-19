using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Pickets;

public partial class PicketWindow
{
    internal PicketItem[] TransferReferences(IEnumerable<PicketItem> items, PicketWindow target)
    {
        if (target == this) return [];
        var before = SnapshotReferences(items);
        var moved = new List<PicketItem>();
        foreach (var item in before.Select(saved => saved.Item))
        {
            if (!Items.Contains(item) || (item.Kind == ItemKind.File && target.Items.Any(i => i.Kind == ItemKind.File &&
                string.Equals(i.Path, item.Path, StringComparison.OrdinalIgnoreCase)))) continue;
            Items.Remove(item);
            item.IsSelected = false;
            target.Items.Add(item);
            moved.Add(item);
        }
        if (moved.Count > 0 && Application.Current is App app)
        {
            var sourceId = PicketId;
            var targetId = target.PicketId;
            var saved = before.Where(entry => moved.Contains(entry.Item)).ToArray();
            app.RecordUndo($"{moved.Count} ta element ko‘chirildi", () =>
            {
                var source = app.Pickets.FirstOrDefault(p => p.PicketId == sourceId);
                var destination = app.Pickets.FirstOrDefault(p => p.PicketId == targetId);
                if (source == null || destination == null || saved.Any(entry => !destination.Items.Contains(entry.Item))) return false;
                if (saved.Any(entry => entry.Item.Kind == ItemKind.File && source.Items.Any(i => i.Kind == ItemKind.File &&
                    string.Equals(i.Path, entry.Item.Path, StringComparison.OrdinalIgnoreCase)))) return false;
                foreach (var entry in saved) destination.Items.Remove(entry.Item);
                return source.RestoreReferences(saved);
            });
        }
        return moved.ToArray();
    }

    private void MoveReferencesTo(IEnumerable<PicketItem> items, PicketWindow target)
    {
        var moved = TransferReferences(items, target);
        if (moved.Length == 0) return;
        target.SetExpanded(true);
        target.FocusForKeyboard();
        target.FocusItem(target.Items.IndexOf(moved[0]));
        foreach (var item in moved) item.IsSelected = true;
    }

    private void AddFiles_Click(object sender, RoutedEventArgs e) => AddFiles();
    private void AddFolder_Click(object sender, RoutedEventArgs e) => AddFolder();

    private void AddFiles()
    {
        var dialog = new OpenFileDialog { Title = "Pickets’ga fayl havolalarini qo‘shish", Multiselect = true, CheckFileExists = true };
        if (dialog.ShowDialog(this) == true) AddReferences(dialog.FileNames, captureDesktop: false);
    }

    private void AddFolder()
    {
        var dialog = new OpenFolderDialog { Title = "Pickets’ga papka havolalarini qo‘shish", Multiselect = true };
        if (dialog.ShowDialog(this) == true) AddReferences(dialog.FolderNames, captureDesktop: false, folders: true);
    }

    internal void AddReferences(IEnumerable<string> paths, bool captureDesktop, bool folders = false)
    {
        var added = new List<PicketItem>();
        foreach (var path in paths)
        {
            if (Items.Any(item => string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase))) continue;
            var item = PicketItem.FromPath(path, folders);
            // References from the Add dialogs leave even desktop icons alone. A desktop drag keeps
            // the existing capture behavior, but never takes a second ownership of an already hidden icon.
            var alreadyCaptured = Application.Current is App app && app.Pickets.SelectMany(p => p.Items)
                .Any(i => i.OriginalDesktopPos.HasValue && string.Equals(i.Path, path, StringComparison.OrdinalIgnoreCase));
            if (captureDesktop && !alreadyCaptured) item.OriginalDesktopPos = DesktopIconHider.Hide(path);
            Items.Add(item);
            added.Add(item);
            _ = item.RefreshAsync();
        }
        SetExpanded(true);
        RecordReferenceAddition(added.ToArray());
    }

    private async void CheckReferences_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in Items.Where(i => i.Kind == ItemKind.File).ToList())
            await item.RefreshAsync();
    }

    private async void ItemMenu_Check_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { DataContext: PicketItem item })
            foreach (var reference in ActionItems(item).Where(i => i.Kind == ItemKind.File)) await reference.RefreshAsync();
    }

    private void ItemMenu_Locate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: PicketItem item }) return;
        if (item.IsFolder) ItemMenu_LocateFolder_Click(sender, e);
        else ItemMenu_LocateFile_Click(sender, e);
    }

    private void ItemMenu_LocateFile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: PicketItem item }) return;
        var dialog = new OpenFileDialog { Title = "Havola qilingan faylni topish", CheckFileExists = true };
        if (dialog.ShowDialog(this) == true) ReplaceReference(item, dialog.FileName, false);
    }

    private void ItemMenu_LocateFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: PicketItem item }) return;
        var dialog = new OpenFolderDialog { Title = "Havola qilingan papkani topish" };
        if (dialog.ShowDialog(this) == true) ReplaceReference(item, dialog.FolderName, true);
    }

    internal void ReplaceReference(PicketItem item, string path, bool folder)
    {
        if (Items.Any(i => i != item && string.Equals(i.Path, path, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(this, "Bu havola ushbu guruhda allaqachon bor.", "Pickets");
            return;
        }
        if (!ReleaseReferenceCapture(item)) return;
        var index = Items.IndexOf(item);
        if (index < 0) return;
        var replacement = PicketItem.FromPath(path, folder);
        replacement.IsLarge = item.IsLarge;
        Items[index] = replacement;
        _ = replacement.RefreshAsync();
        FocusItem(index);
    }

    private bool ReleaseReferenceCapture(PicketItem item)
    {
        if (!item.OriginalDesktopPos.HasValue) return true;
        if (item.Status == ReferenceStatus.Missing || DesktopIconHider.Restore(item.Path, item.OriginalDesktopPos.Value))
        {
            item.OriginalDesktopPos = null;
            return true;
        }
        MessageBox.Show(this, "Windows ish stolidagi asl belgini tiklay olmadi. Havola saqlandi; fayl mavjudligini tekshirib, qayta urinib ko‘ring.",
            "Pickets", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    internal void RemoveReferences(IEnumerable<PicketItem> items)
    {
        var index = ItemsHost.SelectedIndex;
        var removed = new List<RemovedReference>();
        foreach (var saved in SnapshotReferences(items))
            if (ReleaseReferenceCapture(saved.Item)) { Items.Remove(saved.Item); removed.Add(saved); }
        RecordReferenceRemoval(removed.ToArray());
        if (Items.Count > 0) FocusItem(Math.Clamp(index, 0, Items.Count - 1));
        else TitleToggle.Focus();
    }

    internal bool TryReleaseAllReferences()
    {
        foreach (var item in Items)
            if (!ReleaseReferenceCapture(item)) return false;
        return true;
    }

    private static async void LaunchItem(PicketItem item)
    {
        if (item.Kind != ItemKind.File) return;
        await item.RefreshAsync();
        if (item.IsMissing)
        {
            MessageBox.Show($"Ushbu havola hozir mavjud emas:\n{item.Path}\n\nDisk yoki tarmoq ulanishini tiklab, «Qayta tekshirish»ni tanlang. Fayl ko‘chirilgan bo‘lsa, «Joyini ko‘rsatish»ni bosing. Havola saqlangan.",
                "Pickets", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try { Process.Start(new ProcessStartInfo(item.Path) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show($"Ochib bo‘lmadi:\n{item.Path}\n\n{ex.Message}", "Pickets", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
