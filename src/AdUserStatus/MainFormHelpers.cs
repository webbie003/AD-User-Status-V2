using AdUserStatus.Models;

namespace AdUserStatus
{
    internal static class MainFormHelpers
    {

        private static void AddGridContextMenu(DataGridView grid)
        {
            var menu = new ContextMenuStrip();

            var copyEmail = new ToolStripMenuItem("Copy Email", null, (_, __) =>
            {
                if (grid.CurrentRow?.DataBoundItem is UserDto u && !string.IsNullOrWhiteSpace(u.Email))
                    Clipboard.SetText(u.Email);
            });

            var copyRow = new ToolStripMenuItem("Copy Row", null, (_, __) =>
            {
                if (grid.CurrentRow?.DataBoundItem is UserDto u)
                    Clipboard.SetText($"{u.DisplayName}\t{u.Email}\t{u.Enabled}\t{u.Category}");
            });

            menu.Items.AddRange(new[] { copyEmail, copyRow });
            grid.ContextMenuStrip = menu;
        }
    }
}