using SqlFormatter.Properties;
using System;
using System.Linq;
using System.Windows.Forms;

namespace SqlFormatter
{
    public class MainAppContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private SQLFormatter sqlFormatter;
        private const int trayIconBalloonTimeout = 2000;

        internal MainAppContext()
        {
            sqlFormatter = new SQLFormatter();
            sqlFormatter.Notification += SqlFormatter_Notification;

            trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppCon,
                Text = "SQL Formatter",
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true
            };
            BuildMenu(trayIcon.ContextMenuStrip);
            trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick;
        }

        private void BuildMenu(ContextMenuStrip contextMenuStrip)
        {
            BuildMenuItems(contextMenuStrip);
            AddSeparator(contextMenuStrip);
            AddExitMenuItem(contextMenuStrip);
        }

        private void BuildMenuItems(ContextMenuStrip contextMenuStrip)
        {
            ToolStripMenuItem optionsMenu = new ToolStripMenuItem("Options");

            AddChilds(optionsMenu, sqlFormatter.UserFormatOptions);

            contextMenuStrip.Items.Add(optionsMenu);
        }

        private void AddChilds(ToolStripMenuItem optionsMenu, SqlFormatOption[] formatOptions)
        {
            foreach (var formatOption in formatOptions)
            {
                ToolStripMenuItem formatOptionMenu = new ToolStripMenuItem(formatOption.Title);
                formatOptionMenu.CheckOnClick = formatOption.IsCheckable || formatOption.IsRadio;
                formatOptionMenu.Checked = formatOption.IsChecked;
                formatOptionMenu.Tag = formatOption;

                if (formatOptionMenu.CheckOnClick)
                    formatOptionMenu.Click += FormatOptionMenu_Click;

                if (formatOption.Childs.Count > 0)
                    AddChilds(formatOptionMenu, formatOption.Childs.ToArray());

                optionsMenu.DropDownItems.Add(formatOptionMenu);
            }
        }

        private void DisableChildItems(ToolStripMenuItem menuItem)
        {
            foreach (ToolStripMenuItem childMenuItem in menuItem.DropDownItems)
                childMenuItem.Enabled = menuItem.Checked;
        }

        private void AddSeparator(ContextMenuStrip contextMenuStrip)
        {
            contextMenuStrip.Items.Add(new ToolStripSeparator());
        }

        private void AddExitMenuItem(ContextMenuStrip contextMenuStrip)
        {
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;

            contextMenuStrip.Items.Add(exitMenuItem);
        }

        private void HandleRadioCheckItem(ToolStripMenuItem menuItem)
        {
            ToolStripDropDownMenu parentDropDownMenu = menuItem.GetCurrentParent() as ToolStripDropDownMenu;
            ToolStripMenuItem parentMenuItem = parentDropDownMenu.OwnerItem as ToolStripMenuItem;

            foreach (ToolStripMenuItem childMenuItem in parentMenuItem.DropDownItems)
                if (!childMenuItem.Equals(menuItem))
                    childMenuItem.Checked = false;

            menuItem.Checked = true;
        }

        private void SqlFormatter_Notification(string title, string message, ToolTipIcon toolTipIcon)
        {
            if (string.IsNullOrWhiteSpace(title))
                title = toolTipIcon.ToString();

            trayIcon.ShowBalloonTip(trayIconBalloonTimeout, title, message, toolTipIcon);
        }

        private void FormatOptionMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

            if (menuItem == null)
                return;

            SqlFormatOption checkedFormatOption = menuItem.Tag as SqlFormatOption;

            if (checkedFormatOption == null)
                return;

            if (checkedFormatOption.Childs.Any(c => c.IsRadio))
                DisableChildItems(menuItem);

            if (checkedFormatOption.IsRadio)
                HandleRadioCheckItem(menuItem);

            checkedFormatOption.IsChecked = menuItem.Checked;

            sqlFormatter.QueuePendingChanges(checkedFormatOption);
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                sqlFormatter.Format();
            }
            catch (Exception ex)
            {
                SqlFormatter_Notification("Unhandled error", ex.Message, ToolTipIcon.Error);
            }
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

            if (menuItem == null)
                return;

            ContextMenuStrip contextMenu = menuItem.GetCurrentParent() as ContextMenuStrip;

            if (contextMenu == null)
                return;

            contextMenu.Close();

            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}
