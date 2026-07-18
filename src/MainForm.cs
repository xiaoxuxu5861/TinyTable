using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TinyTable
{
    public sealed class MainForm : Form
    {
        private readonly DesktopIconService _desktop;
        private readonly SnapshotStore _store;
        private readonly SelectionListService _selectionList;
        private SnapshotDatabase _database;
        private readonly ListView _iconList;
        private readonly ListBox _snapshotList;
        private readonly Label _desktopSummary;
        private readonly ComboBox _selectionPreset;
        private readonly Label _snapshotSummary;
        private readonly ToolStripStatusLabel _status;
        private readonly bool _startedByWindows;
        private readonly System.Windows.Forms.Timer _autoSaveTimer;
        private NotifyIcon _trayIcon;
        private List<DesktopIcon> _icons;
        private bool _refreshingChecks;
        private bool _allowExit;
        private bool _shutdownSaving;
        private string _lastLayoutSignature;

        public MainForm() : this(false) { }

        public MainForm(bool startedByWindows)
        {
            _startedByWindows = startedByWindows;
            _desktop = new DesktopIconService();
            _store = new SnapshotStore();
            _selectionList = new SelectionListService();
            _database = _store.Load();
            if (_database.DisplaySortMode == DesktopListSortMode.Date)
            {
                _database.DisplaySortMode = DesktopListSortMode.DesktopPosition;
                _database.DisplaySortDescending = false;
            }
            Localization.SetLanguage(Automation.LanguageCode);
            _icons = new List<DesktopIcon>();

            Text = "TinyTable";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(900, 520);
            ClientSize = new Size(960, 620);
            BackColor = Color.FromArgb(239, 242, 244);
            Font = new Font("Microsoft YaHei UI", 9f);
            try
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    typeof(MainForm).Assembly.Location);
            }
            catch { }
            RestoreSavedWindowBounds();

            Panel masthead = BuildMasthead();
            masthead.Dock = DockStyle.Fill;
            masthead.Margin = Padding.Empty;

            TableLayoutPanel rootLayout = new TableLayoutPanel();
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.ColumnCount = 1;
            rootLayout.RowCount = 3;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62f));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
            rootLayout.Margin = Padding.Empty;
            rootLayout.Padding = Padding.Empty;
            Controls.Add(rootLayout);
            rootLayout.Controls.Add(masthead, 0, 0);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Margin = Padding.Empty;
            split.BackColor = Color.FromArgb(211, 218, 224);
            split.Panel1.BackColor = Color.White;
            split.Panel2.BackColor = Color.FromArgb(247, 248, 249);
            rootLayout.Controls.Add(split, 0, 1);

            TableLayoutPanel leftLayout = new TableLayoutPanel();
            leftLayout.Dock = DockStyle.Fill;
            leftLayout.ColumnCount = 1;
            leftLayout.RowCount = 2;
            leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72f));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            leftLayout.Margin = Padding.Empty;
            leftLayout.Padding = Padding.Empty;
            split.Panel1.Controls.Add(leftLayout);

            Panel leftToolbar = new Panel();
            leftToolbar.Dock = DockStyle.Fill;
            leftToolbar.Margin = Padding.Empty;
            leftToolbar.Padding = new Padding(14, 5, 8, 3);
            leftToolbar.BackColor = Color.White;
            leftLayout.Controls.Add(leftToolbar, 0, 0);

            TableLayoutPanel leftToolbarLayout = new TableLayoutPanel();
            leftToolbarLayout.Dock = DockStyle.Fill;
            leftToolbarLayout.Margin = Padding.Empty;
            leftToolbarLayout.Padding = Padding.Empty;
            leftToolbarLayout.ColumnCount = 1;
            leftToolbarLayout.RowCount = 2;
            leftToolbarLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100f));
            leftToolbarLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 27f));
            leftToolbarLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 35f));
            leftToolbar.Controls.Add(leftToolbarLayout);

            _desktopSummary = new Label();
            _desktopSummary.Dock = DockStyle.Fill;
            _desktopSummary.Margin = Padding.Empty;
            _desktopSummary.Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold);
            _desktopSummary.ForeColor = Color.FromArgb(28, 37, 48);
            _desktopSummary.TextAlign = ContentAlignment.MiddleLeft;
            leftToolbarLayout.Controls.Add(_desktopSummary, 0, 0);

            FlowLayoutPanel presetRow = new FlowLayoutPanel();
            presetRow.Dock = DockStyle.Fill;
            presetRow.Margin = Padding.Empty;
            presetRow.FlowDirection = FlowDirection.LeftToRight;
            presetRow.WrapContents = false;
            leftToolbarLayout.Controls.Add(presetRow, 0, 1);

            presetRow.Controls.Add(MakeButton("刷新桌面", RefreshDesktop, false));
            presetRow.Controls.Add(MakeButton("读取桌面选中项", SelectExplorerSelection, false));
            presetRow.Controls.Add(MakeButton("快速选择 ▾", ShowQuickSelectMenu, false));
            presetRow.Controls.Add(MakeButton("备份勾选项目", SaveCheckedSnapshot, true));

            _selectionPreset = new ComboBox();
            _selectionPreset.DropDownStyle = ComboBoxStyle.DropDownList;
            _selectionPreset.Width = 124;
            _selectionPreset.Margin = new Padding(0, 3, 8, 0);
            presetRow.Controls.Add(MakeButton("更多 ▾", ShowSelectionMenu, false));

            _iconList = new ListView();
            _iconList.Dock = DockStyle.Fill;
            _iconList.Margin = Padding.Empty;
            _iconList.View = View.Details;
            _iconList.CheckBoxes = true;
            _iconList.FullRowSelect = true;
            _iconList.HideSelection = false;
            _iconList.BorderStyle = BorderStyle.None;
            _iconList.BackColor = Color.White;
            _iconList.ForeColor = Color.FromArgb(38, 47, 58);
            _iconList.Columns.Add("备份", 52);
            _iconList.Columns.Add("桌面项目", 260);
            _iconList.Columns.Add("类型", 76);
            _iconList.Columns.Add("当前位置", 96);
            _iconList.Columns.Add("来源", 120);
            _iconList.ItemChecked += OnIconItemChecked;
            _iconList.ColumnClick += OnIconColumnClick;
            leftLayout.Controls.Add(_iconList, 0, 1);

            TableLayoutPanel rightLayout = new TableLayoutPanel();
            rightLayout.Dock = DockStyle.Fill;
            rightLayout.ColumnCount = 1;
            rightLayout.RowCount = 2;
            rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72f));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rightLayout.Margin = Padding.Empty;
            rightLayout.Padding = Padding.Empty;
            split.Panel2.Controls.Add(rightLayout);

            Panel rightHeader = new Panel();
            rightHeader.Dock = DockStyle.Fill;
            rightHeader.Margin = Padding.Empty;
            rightHeader.Padding = new Padding(14, 6, 8, 4);
            rightHeader.BackColor = Color.FromArgb(247, 248, 249);
            rightLayout.Controls.Add(rightHeader, 0, 0);

            _snapshotSummary = new Label();
            _snapshotSummary.AutoSize = true;
            _snapshotSummary.Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold);
            _snapshotSummary.ForeColor = Color.FromArgb(28, 37, 48);
            _snapshotSummary.Location = new Point(12, 7);
            rightHeader.Controls.Add(_snapshotSummary);

            FlowLayoutPanel rightButtons = new FlowLayoutPanel();
            rightButtons.AutoSize = true;
            rightButtons.Location = new Point(9, 37);
            rightButtons.WrapContents = false;
            rightHeader.Controls.Add(rightButtons);
            rightButtons.Controls.Add(MakeButton("恢复", RestoreSelectedSnapshot, true));
            rightButtons.Controls.Add(MakeButton("快照操作 ▾", ShowSnapshotMenu, false));

            _snapshotList = new ListBox();
            _snapshotList.Dock = DockStyle.Fill;
            _snapshotList.Margin = Padding.Empty;
            _snapshotList.BorderStyle = BorderStyle.None;
            _snapshotList.BackColor = Color.FromArgb(247, 248, 249);
            _snapshotList.ForeColor = Color.FromArgb(38, 47, 58);
            _snapshotList.Font = new Font("Microsoft YaHei UI", 9.5f);
            _snapshotList.ItemHeight = 30;
            _snapshotList.DoubleClick += delegate { RestoreSelectedSnapshot(null, EventArgs.Empty); };
            rightLayout.Controls.Add(_snapshotList, 0, 1);

            StatusStrip statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(29, 39, 51);
            statusStrip.ForeColor = Color.FromArgb(224, 230, 235);
            statusStrip.Dock = DockStyle.Fill;
            statusStrip.Margin = Padding.Empty;
            statusStrip.SizingGrip = false;
            _status = new ToolStripStatusLabel();
            _status.Spring = true;
            _status.TextAlign = ContentAlignment.MiddleLeft;
            statusStrip.Items.Add(_status);
            rootLayout.Controls.Add(statusStrip, 0, 2);

            _autoSaveTimer = new System.Windows.Forms.Timer();
            _autoSaveTimer.Tick += delegate { RunAutomaticSave("定时保存", false); };
            ConfigureAutomation();
            SystemEvents.SessionEnding += OnSessionEnding;
            SystemEvents.SessionSwitch += OnSessionSwitch;
            Localization.Apply(this);
            UpdateColumnSortIndicators();
            RefreshSelectionPresets();

            Shown += delegate
            {
                split.SplitterDistance = (int)(split.ClientSize.Width * 0.67);
                RefreshSnapshotList();
                RefreshDesktop(null, EventArgs.Empty);
                RunStartupActions();
            };
        }

        private Panel BuildMasthead()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Height = 62;
            panel.BackColor = Color.FromArgb(17, 25, 35);

            Panel accent = new Panel();
            accent.Dock = DockStyle.Left;
            accent.Width = 5;
            accent.BackColor = Color.FromArgb(77, 211, 176);
            panel.Controls.Add(accent);

            PictureBox logo = new PictureBox();
            logo.Size = new Size(32, 32);
            logo.Location = new Point(20, 15);
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.BackColor = Color.Transparent;
            if (Icon != null)
                logo.Image = Icon.ToBitmap();
            panel.Controls.Add(logo);

            Label title = new Label();
            title.Text = "TinyTable";
            title.AutoSize = true;
            title.Font = new Font("Microsoft YaHei UI", 15f, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(64, 5);
            panel.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "局部桌面备份  ·  只保护你选择的图标位置";
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Microsoft YaHei UI", 9f);
            subtitle.ForeColor = Color.FromArgb(154, 169, 184);
            subtitle.Location = new Point(66, 35);
            panel.Controls.Add(subtitle);
            return panel;
        }

        private Button MakeButton(
            string text, EventHandler handler, bool primary)
        {
            Button button = new Button();
            button.Text = text;
            button.AutoSize = true;
            button.Height = 29;
            button.Margin = new Padding(3, 0, 3, 0);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(194, 203, 211);
            button.BackColor = primary
                ? Color.FromArgb(28, 137, 113)
                : Color.FromArgb(250, 251, 252);
            button.ForeColor = primary ? Color.White : Color.FromArgb(42, 52, 63);
            button.Click += handler;
            return button;
        }

        private void ShowQuickSelectMenu(object sender, EventArgs e)
        {
            Dictionary<string, int> counts = DesktopItemClassifier.CategoryOrder
                .ToDictionary(
                    category => category,
                    category => _iconList.Items.Cast<ListViewItem>().Count(x =>
                        string.Equals(((DesktopIcon)x.Tag).Category, category,
                            StringComparison.OrdinalIgnoreCase)),
                    StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkedKeys = new HashSet<string>(
                _iconList.CheckedItems.Cast<ListViewItem>()
                    .Select(x => ((DesktopIcon)x.Tag).Key),
                StringComparer.OrdinalIgnoreCase);
            List<string> selectedCategories = DesktopItemClassifier.CategoryOrder
                .Where(category => counts[category] > 0
                    && _iconList.Items.Cast<ListViewItem>()
                        .Where(x => string.Equals(((DesktopIcon)x.Tag).Category,
                            category, StringComparison.OrdinalIgnoreCase))
                        .All(x => checkedKeys.Contains(((DesktopIcon)x.Tag).Key)))
                .ToList();

            using (QuickSelectionDialog dialog =
                new QuickSelectionDialog(counts, selectedCategories))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    SelectCategories(dialog.SelectedCategories);
            }
        }

        private void SelectCategories(IEnumerable<string> categories)
        {
            HashSet<string> selectedCategories = new HashSet<string>(
                categories ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            List<string> keys = new List<string>();
            _refreshingChecks = true;
            foreach (ListViewItem item in _iconList.Items)
            {
                DesktopIcon icon = (DesktopIcon)item.Tag;
                bool selected = selectedCategories.Contains(icon.Category);
                item.Checked = selected;
                if (selected)
                    keys.Add(icon.Key);
            }
            _refreshingChecks = false;
            try
            {
                ReplaceProtectedKeys(keys, null);
                _selectionPreset.SelectedIndex = 0;
                _status.Text = Localization.T("已快速选择") + " "
                    + keys.Count + " " + Localization.T("项") + " · "
                    + selectedCategories.Count + " "
                    + Localization.T("种类型");
            }
            catch (Exception ex)
            {
                ShowError("更新备份列表失败", ex.Message);
            }
        }

        private void ShowSelectionMenu(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem loadMenu = new ToolStripMenuItem(Localization.T("载入"));
            ToolStripMenuItem deleteMenu = new ToolStripMenuItem(Localization.T("删除方案"));
            List<SelectionPreset> presets = (_database.SelectionPresets
                ?? new List<SelectionPreset>())
                .OrderByDescending(x => x.UpdatedUtc).ToList();
            foreach (SelectionPreset preset in presets)
            {
                SelectionPreset selectedPreset = preset;
                loadMenu.DropDownItems.Add(preset.Name, null, delegate
                {
                    SelectPresetInCombo(selectedPreset.Id);
                    LoadSelectedPreset(null, EventArgs.Empty);
                });
                deleteMenu.DropDownItems.Add(preset.Name, null, delegate
                {
                    SelectPresetInCombo(selectedPreset.Id);
                    DeleteSelectedPreset(null, EventArgs.Empty);
                });
            }
            loadMenu.Enabled = presets.Count > 0;
            deleteMenu.Enabled = presets.Count > 0;
            menu.Items.Add(loadMenu);
            menu.Items.Add(Localization.T("保存方案"), null, SaveSelectionPreset);
            menu.Items.Add(deleteMenu);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Localization.T("刷新右键菜单"), null, RefreshContextMenu);
            menu.Items.Add(Localization.T("设置"), null, ShowAutomationSettings);
            ShowMenuBelowButton(menu, sender as Button);
        }

        private void SelectPresetInCombo(string presetId)
        {
            for (int i = 0; i < _selectionPreset.Items.Count; i++)
            {
                SelectionPresetListItem item =
                    _selectionPreset.Items[i] as SelectionPresetListItem;
                if (item != null && item.Preset != null
                    && string.Equals(item.Preset.Id, presetId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _selectionPreset.SelectedIndex = i;
                    return;
                }
            }
        }

        private void ShowSnapshotMenu(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(Localization.T("删除快照"), null, DeleteSelectedSnapshot);
            menu.Items.Add(Localization.T("撤销恢复"), null, UndoLastRestore);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Localization.T("设置"), null, ShowAutomationSettings);
            ShowMenuBelowButton(menu, sender as Button);
        }

        private static void ShowMenuBelowButton(ContextMenuStrip menu, Button button)
        {
            if (button == null)
            {
                menu.Dispose();
                return;
            }
            menu.Closed += delegate
            {
                // ToolStripDropDown.OnItemClicked still uses the menu handle
                // after Closed fires. Dispose on the next UI message instead
                // of invalidating the handle inside the click call stack.
                if (!button.IsDisposed && button.IsHandleCreated)
                {
                    try
                    {
                        button.BeginInvoke(new Action(menu.Dispose));
                    }
                    catch (InvalidOperationException)
                    {
                        menu.Dispose();
                    }
                }
                else
                {
                    menu.Dispose();
                }
            };
            menu.Show(button, new Point(0, button.Height));
        }

        private void OnIconColumnClick(object sender, ColumnClickEventArgs e)
        {
            DesktopListSortMode mode;
            switch (e.Column)
            {
                case 1: mode = DesktopListSortMode.Name; break;
                case 2: mode = DesktopListSortMode.FileType; break;
                case 3: mode = DesktopListSortMode.DesktopPosition; break;
                case 4: mode = DesktopListSortMode.Source; break;
                default: return;
            }
            if (_database.DisplaySortMode == mode)
                _database.DisplaySortDescending =
                    !_database.DisplaySortDescending;
            else
            {
                _database.DisplaySortMode = mode;
                _database.DisplaySortDescending = false;
            }
            try { _store.Save(_database); } catch { }
            UpdateColumnSortIndicators();
            RefreshDesktop(null, EventArgs.Empty);
        }

        private IEnumerable<DesktopIcon> OrderDisplayedIcons(
            IEnumerable<DesktopIcon> icons)
        {
            bool descending = _database.DisplaySortDescending;
            switch (_database.DisplaySortMode)
            {
                case DesktopListSortMode.FileType:
                    return descending
                        ? icons.OrderByDescending(x => CategoryOrderIndex(x.Category))
                            .ThenByDescending(x => x.DisplayName,
                                StringComparer.CurrentCultureIgnoreCase)
                        : icons.OrderBy(x => CategoryOrderIndex(x.Category))
                            .ThenBy(x => x.DisplayName,
                                StringComparer.CurrentCultureIgnoreCase);
                case DesktopListSortMode.Name:
                    return descending
                        ? icons.OrderByDescending(x => x.DisplayName,
                            StringComparer.CurrentCultureIgnoreCase)
                        : icons.OrderBy(x => x.DisplayName,
                            StringComparer.CurrentCultureIgnoreCase);
                case DesktopListSortMode.Source:
                    return descending
                        ? icons.OrderByDescending(x => SourceOrderIndex(x))
                            .ThenByDescending(x => x.DisplayName,
                                StringComparer.CurrentCultureIgnoreCase)
                        : icons.OrderBy(x => SourceOrderIndex(x))
                            .ThenBy(x => x.DisplayName,
                                StringComparer.CurrentCultureIgnoreCase);
                default:
                    return descending
                        ? icons.OrderByDescending(x => x.X).ThenByDescending(x => x.Y)
                        : icons.OrderBy(x => x.X).ThenBy(x => x.Y);
            }
        }

        private void UpdateColumnSortIndicators()
        {
            if (_iconList == null || _iconList.Columns.Count < 5)
                return;
            string[] titles = { "备份", "桌面项目", "类型", "当前位置", "来源" };
            for (int i = 0; i < titles.Length; i++)
                _iconList.Columns[i].Text = Localization.T(titles[i]);
            int column = -1;
            switch (_database.DisplaySortMode)
            {
                case DesktopListSortMode.Name: column = 1; break;
                case DesktopListSortMode.FileType: column = 2; break;
                case DesktopListSortMode.DesktopPosition: column = 3; break;
                case DesktopListSortMode.Source: column = 4; break;
            }
            if (column >= 0)
                _iconList.Columns[column].Text +=
                    _database.DisplaySortDescending ? "  ▼" : "  ▲";
        }

        private static int SourceOrderIndex(DesktopIcon icon)
        {
            if (icon.IsVirtual)
                return 2;
            string personal = Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory);
            return !string.IsNullOrEmpty(icon.Path)
                && icon.Path.StartsWith(personal, StringComparison.OrdinalIgnoreCase)
                ? 0 : 1;
        }

        private static int CategoryOrderIndex(string category)
        {
            for (int i = 0; i < DesktopItemClassifier.CategoryOrder.Length; i++)
            {
                if (string.Equals(
                    DesktopItemClassifier.CategoryOrder[i],
                    category,
                    StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return DesktopItemClassifier.CategoryOrder.Length;
        }

        private AutomationSettings Automation
        {
            get
            {
                if (_database.AutomationSettings == null)
                    _database.AutomationSettings = AutomationSettings.CreateDefault();
                return _database.AutomationSettings;
            }
        }

        private void RestoreSavedWindowBounds()
        {
            AutomationSettings settings = Automation;
            if (!settings.SaveWindowPosition
                || settings.WindowWidth < 700
                || settings.WindowHeight < 500)
                return;
            Rectangle bounds = new Rectangle(
                settings.WindowX,
                settings.WindowY,
                settings.WindowWidth,
                settings.WindowHeight);
            bool visible = Screen.AllScreens.Any(x =>
                x.WorkingArea.IntersectsWith(bounds));
            if (!visible)
                return;
            StartPosition = FormStartPosition.Manual;
            Bounds = bounds;
        }

        private void SaveWindowBounds()
        {
            if (!Automation.SaveWindowPosition)
                return;
            Rectangle bounds = WindowState == FormWindowState.Normal
                ? Bounds : RestoreBounds;
            Automation.WindowX = bounds.X;
            Automation.WindowY = bounds.Y;
            Automation.WindowWidth = bounds.Width;
            Automation.WindowHeight = bounds.Height;
        }

        private void ConfigureAutomation()
        {
            AutomationSettings settings = Automation;
            try { StartupService.Apply(settings.StartWithWindows); } catch { }

            _autoSaveTimer.Stop();
            if (settings.AutoSaveEnabled)
            {
                int minutes = Math.Max(1, settings.AutoSaveMinutes);
                _autoSaveTimer.Interval = checked(minutes * 60 * 1000);
                _autoSaveTimer.Start();
            }
            ConfigureTray();
        }

        private void ConfigureTray()
        {
            if (_trayIcon == null)
            {
                _trayIcon = new NotifyIcon();
                _trayIcon.Icon = Icon;
                _trayIcon.Text = "TinyTable";
                _trayIcon.DoubleClick += delegate { ShowMainWindow(); };
            }

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Font = new Font("Microsoft YaHei UI", 9f);
            menu.Items.Add(Localization.T("打开 TinyTable"), null, delegate { ShowMainWindow(); });
            menu.Items.Add(Localization.T("立即自动保存"), null, delegate
            {
                RunAutomaticSave("手动自动保存", true);
            });
            menu.Items.Add(Localization.T("恢复最新快照"), null, delegate
            {
                RunAutomaticRestore("托盘恢复");
            });
            menu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem autoSave = new ToolStripMenuItem(
                Localization.T("启用自动保存"));
            autoSave.Checked = Automation.AutoSaveEnabled;
            autoSave.Click += delegate
            {
                Automation.AutoSaveEnabled = !Automation.AutoSaveEnabled;
                _store.Save(_database);
                BeginInvoke(new Action(ConfigureAutomation));
            };
            menu.Items.Add(autoSave);
            menu.Items.Add(Localization.T("自动化与常驻设置…"), null, ShowAutomationSettings);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Localization.T("退出 TinyTable"), null, delegate { ExitApplication(); });
            if (_trayIcon.ContextMenuStrip != null)
                _trayIcon.ContextMenuStrip.Dispose();
            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.Visible = Automation.TrayEnabled;
        }

        private void ShowAutomationSettings(object sender, EventArgs e)
        {
            AutomationSettings old = Automation;
            using (AutomationSettingsDialog dialog =
                new AutomationSettingsDialog(old))
            {
                dialog.LanguagePreviewChanged += delegate
                {
                    ApplyCurrentLanguage();
                };
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                AutomationSettings updated = dialog.SelectedSettings;
                updated.WindowX = old.WindowX;
                updated.WindowY = old.WindowY;
                updated.WindowWidth = old.WindowWidth;
                updated.WindowHeight = old.WindowHeight;
                _database.AutomationSettings = updated;
            }
            bool languageChanged = !string.Equals(
                old.LanguageCode,
                Automation.LanguageCode,
                StringComparison.OrdinalIgnoreCase);
            Localization.SetLanguage(Automation.LanguageCode);
            SaveWindowBounds();
            _store.Save(_database);
            ConfigureAutomation();
            ApplyCurrentLanguage();
            try { ShellContextMenuService.Install(); } catch { }
            _status.Text = languageChanged
                ? Localization.T("语言设置已立即应用。")
                : Localization.T("自动化与常驻设置已保存。");
        }

        private void ApplyCurrentLanguage()
        {
            Localization.Apply(this);
            UpdateColumnSortIndicators();
            RefreshSelectionPresets();
            RefreshSnapshotList();
            RefreshDesktop(null, EventArgs.Empty);
            ConfigureTray();
            _status.Text = Localization.T("界面语言已切换。");
        }

        private void RunStartupActions()
        {
            if (Automation.DisableAutoArrangeOnStart)
            {
                try { _desktop.DisableAutoArrange(); } catch { }
            }

            bool shouldRestore = Automation.RestoreOnProgramStart
                || (_startedByWindows && Automation.RestoreOnWindowsStart);
            if (shouldRestore)
                RunAutomaticRestore(_startedByWindows ? "Windows 启动恢复" : "程序启动恢复");

            if (Automation.TrayEnabled && Automation.StartMinimized)
            {
                BeginInvoke(new Action(delegate
                {
                    Hide();
                    ShowInTaskbar = false;
                }));
            }
        }

        private bool RunAutomaticSave(string reason, bool force)
        {
            try
            {
                List<DesktopIcon> current = _desktop.ReadIcons();
                HashSet<string> protectedKeys = new HashSet<string>(
                    _database.ProtectedKeys, StringComparer.OrdinalIgnoreCase);
                List<DesktopIcon> selected = current
                    .Where(x => protectedKeys.Contains(x.Key)).ToList();
                if (selected.Count == 0)
                {
                    _status.Text = "自动保存已跳过：备份列表中没有当前可见的桌面项目。";
                    return false;
                }

                string signature = BuildLayoutSignature(current);
                if (!force && Automation.OnlyWhenLayoutChanged
                    && string.Equals(signature, _lastLayoutSignature, StringComparison.Ordinal))
                    return false;

                IconPosition desktopSize = _desktop.GetDesktopSize();
                LayoutSnapshot snapshot = TinyTable.LayoutEngine.Capture(
                    BuildAutomaticSnapshotName(reason, desktopSize),
                    current,
                    selected,
                    desktopSize,
                    _desktop.GetSpacing());
                snapshot.IsAutoSave = true;
                _database.Snapshots.Insert(0, snapshot);
                PruneAutomaticSnapshots();
                _lastLayoutSignature = signature;
                _store.Save(_database);
                RefreshSnapshotList();
                _status.Text = reason + "完成：保护 " + selected.Count + " 个图标位置。";
                ShowTrayMessage("TinyTable 已保存", snapshot.Name);
                return true;
            }
            catch (Exception ex)
            {
                _status.Text = reason + "失败：" + ex.Message;
                ShowTrayMessage("TinyTable 保存失败", ex.Message);
                return false;
            }
        }

        private void PruneAutomaticSnapshots()
        {
            int keep = Automation.AutoSaveRetention;
            if (keep <= 0)
                return;
            List<LayoutSnapshot> remove = _database.Snapshots
                .Where(x => x.IsAutoSave)
                .OrderByDescending(x => x.CreatedUtc)
                .Skip(keep)
                .ToList();
            foreach (LayoutSnapshot snapshot in remove)
                _database.Snapshots.Remove(snapshot);
        }

        private string BuildAutomaticSnapshotName(
            string reason, IconPosition desktopSize)
        {
            string core;
            switch (Automation.NamingMode)
            {
                case SnapshotNamingMode.Resolution:
                    Rectangle screen = Screen.PrimaryScreen.Bounds;
                    core = screen.Width + "x" + screen.Height;
                    break;
                case SnapshotNamingMode.WorkArea:
                    core = desktopSize.X + "x" + desktopSize.Y;
                    break;
                case SnapshotNamingMode.CustomPrefix:
                    core = string.IsNullOrWhiteSpace(Automation.CustomNamePrefix)
                        ? "TinyTable" : Automation.CustomNamePrefix.Trim();
                    break;
                default:
                    core = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
                    break;
            }
            return "自动 · " + core + " · "
                + DateTime.Now.ToString("MM-dd HH:mm") + " · " + reason;
        }

        private string BuildManualSnapshotName()
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
            switch (Automation.NamingMode)
            {
                case SnapshotNamingMode.Resolution:
                    Rectangle screen = Screen.PrimaryScreen.Bounds;
                    return screen.Width + "x" + screen.Height + " " + time;
                case SnapshotNamingMode.WorkArea:
                    IconPosition size = _desktop.GetDesktopSize();
                    return size.X + "x" + size.Y + " " + time;
                case SnapshotNamingMode.CustomPrefix:
                    return (string.IsNullOrWhiteSpace(Automation.CustomNamePrefix)
                        ? "TinyTable" : Automation.CustomNamePrefix.Trim()) + " " + time;
                default:
                    return time;
            }
        }

        private static string BuildLayoutSignature(IEnumerable<DesktopIcon> icons)
        {
            return string.Join("|", icons
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.Key + ":" + x.X + "," + x.Y)
                .ToArray());
        }

        private void RunAutomaticRestore(string reason)
        {
            LayoutSnapshot snapshot = _database.Snapshots
                .OrderByDescending(x => x.CreatedUtc).FirstOrDefault();
            if (snapshot == null)
            {
                ShowTrayMessage("TinyTable", "没有可恢复的快照。");
                return;
            }
            try
            {
                if (_desktop.IsAutoArrangeEnabled())
                {
                    if (Automation.DisableAutoArrangeBeforeRestore)
                        _desktop.DisableAutoArrange();
                    if (_desktop.IsAutoArrangeEnabled())
                    {
                        ShowTrayMessage("TinyTable 无法恢复",
                            "请先关闭桌面的“自动排列图标”。");
                        return;
                    }
                }

                List<DesktopIcon> current = _desktop.ReadIcons();
                IconPosition size = _desktop.GetDesktopSize();
                IconPosition spacing = _desktop.GetSpacing();
                RestorePlan plan = TinyTable.LayoutEngine.BuildPlan(
                    snapshot, current, size, spacing, _database.RestorePreferences);
                _database.UndoSnapshot = TinyTable.LayoutEngine.Capture(
                    "自动恢复前备份", current, current, size, spacing);
                _store.Save(_database);
                int applied = _desktop.ApplyPositions(plan.Positions);
                RefreshDesktop(null, EventArgs.Empty);
                _status.Text = reason + "完成：定位 " + applied + " 个项目。";
                string detail = applied + " 个项目已定位";
                if (plan.Overflow.Count > 0)
                    detail += "；" + plan.Overflow.Count + " 个项目空间不足，未移动文件";
                ShowTrayMessage("TinyTable 自动恢复", detail);
            }
            catch (Exception ex)
            {
                ShowTrayMessage("TinyTable 恢复失败", ex.Message);
            }
        }

        private void ShowTrayMessage(string title, string text)
        {
            if (_trayIcon == null || !Automation.TrayEnabled)
                return;
            _trayIcon.BalloonTipTitle = title;
            _trayIcon.BalloonTipText = text.Length > 240
                ? text.Substring(0, 240) : text;
            _trayIcon.ShowBalloonTip(3500);
        }

        private void ShowMainWindow()
        {
            ShowInTaskbar = true;
            Show();
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
        }

        private void ExitApplication()
        {
            _allowExit = true;
            Close();
        }

        private void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(delegate { OnSessionEnding(sender, e); }));
                }
                catch { }
                return;
            }
            _shutdownSaving = true;
            if (Automation.SaveOnShutdown)
                RunAutomaticSave("Windows 关机保存", false);
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(delegate { OnSessionSwitch(sender, e); }));
                }
                catch { }
                return;
            }

            bool leaving = e.Reason == SessionSwitchReason.SessionLock
                || e.Reason == SessionSwitchReason.SessionLogoff
                || e.Reason == SessionSwitchReason.ConsoleDisconnect
                || e.Reason == SessionSwitchReason.RemoteDisconnect;
            bool returning = e.Reason == SessionSwitchReason.SessionUnlock
                || e.Reason == SessionSwitchReason.SessionLogon
                || e.Reason == SessionSwitchReason.ConsoleConnect
                || e.Reason == SessionSwitchReason.RemoteConnect;
            if (leaving && Automation.SaveOnSessionLock)
                RunAutomaticSave("切换用户保存", false);
            else if (returning && Automation.RestoreOnSessionUnlock)
                RunAutomaticRestore("切回用户恢复");
        }

        private void RefreshDesktop(object sender, EventArgs e)
        {
            try
            {
                HashSet<string> checkedKeys = new HashSet<string>(
                    _iconList.Items.Cast<ListViewItem>()
                        .Where(x => x.Checked)
                        .Select(x => ((DesktopIcon)x.Tag).Key),
                    StringComparer.OrdinalIgnoreCase);
                foreach (string key in _database.ProtectedKeys)
                    checkedKeys.Add(key);
                _icons = _desktop.ReadIcons();
                _refreshingChecks = true;
                _iconList.BeginUpdate();
                _iconList.Items.Clear();
                foreach (DesktopIcon icon in OrderDisplayedIcons(_icons))
                {
                    ListViewItem item = new ListViewItem(string.Empty);
                    item.SubItems.Add(icon.DisplayName);
                    item.SubItems.Add(Localization.T(icon.Category));
                    item.SubItems.Add(icon.X + ", " + icon.Y);
                    item.SubItems.Add(icon.IsVirtual
                        ? "Windows Shell"
                        : ShortPath(icon.Path));
                    item.Tag = icon;
                    item.Checked = checkedKeys.Contains(icon.Key);
                    _iconList.Items.Add(item);
                }
                _iconList.EndUpdate();
                _refreshingChecks = false;
                bool autoArrange = _desktop.IsAutoArrangeEnabled();
                _desktopSummary.Text = _icons.Count + " "
                    + Localization.T("个桌面项目")
                    + (autoArrange ? "  ·  自动排列已开启" : "  ·  可自由定位");
                _status.Text = autoArrange
                    ? "请先右键桌面 → 查看 → 关闭“自动排列图标”，否则 Windows 不允许恢复单个坐标。"
                    : "桌面读取完成。勾选需要保护位置的项目，然后创建快照。";
            }
            catch (Exception ex)
            {
                _refreshingChecks = false;
                ShowError("读取桌面失败", ex.Message);
            }
        }

        private void SelectExplorerSelection(object sender, EventArgs e)
        {
            try
            {
                _icons = _desktop.ReadIcons();
                HashSet<string> selected = new HashSet<string>(
                    _icons.Where(x => x.IsSelected).Select(x => x.Key),
                    StringComparer.OrdinalIgnoreCase);
                _refreshingChecks = true;
                foreach (ListViewItem item in _iconList.Items)
                    item.Checked = selected.Contains(((DesktopIcon)item.Tag).Key);
                _refreshingChecks = false;
                ReplaceProtectedKeys(selected, null);
                _selectionPreset.SelectedIndex = 0;
                _status.Text = selected.Count == 0
                    ? "Windows 当前没有选中的桌面图标。"
                    : "已读取 Windows 桌面选择：" + selected.Count + " 项。";
            }
            catch (Exception ex)
            {
                _refreshingChecks = false;
                ShowError("读取选择失败", ex.Message);
            }
        }

        private void SetAllChecks(bool value)
        {
            _refreshingChecks = true;
            foreach (ListViewItem item in _iconList.Items)
                item.Checked = value;
            _refreshingChecks = false;
            try
            {
                ReplaceProtectedKeys(value
                    ? _iconList.Items.Cast<ListViewItem>()
                        .Select(x => ((DesktopIcon)x.Tag).Key)
                    : Enumerable.Empty<string>(), null);
                _selectionPreset.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError("更新备份列表失败", ex.Message);
            }
        }

        private void ClearProtectedSelection(object sender, EventArgs e)
        {
            List<string> current = (_database.ProtectedKeys ?? new List<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (current.Count == 0)
            {
                _status.Text = Localization.T("当前选择已经是空的。");
                return;
            }

            SelectionPreset recovery = SaveSelectionPresetInternal(
                Localization.T("清空前") + " " + DateTime.Now.ToString("MM-dd HH:mm"),
                current,
                true);
            SetAllChecks(false);
            RefreshSelectionPresets();
            _status.Text = Localization.T("选择已清空；可从方案中恢复：") + recovery.Name;
        }

        private void SaveSelectionPreset(object sender, EventArgs e)
        {
            List<string> keys = _iconList.CheckedItems
                .Cast<ListViewItem>()
                .Select(x => ((DesktopIcon)x.Tag).Key)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (keys.Count == 0)
            {
                MessageBox.Show(
                    Localization.T("当前没有勾选项目，不能保存选择方案。"),
                    "TinyTable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectionPreset selected = SelectedSelectionPreset();
            string defaultName = selected == null || selected.IsAutomatic
                ? Localization.T("选择方案") + " " + DateTime.Now.ToString("MM-dd HH-mm")
                : selected.Name;
            string name = PromptForName(Localization.T("保存选择方案"), defaultName);
            if (string.IsNullOrWhiteSpace(name))
                return;

            SelectionPreset preset = (_database.SelectionPresets ?? new List<SelectionPreset>())
                .FirstOrDefault(x => string.Equals(
                    x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (preset != null && preset != selected)
            {
                if (MessageBox.Show(
                    Localization.T("已有同名方案，是否用当前选择覆盖它？"),
                    "TinyTable", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question) != DialogResult.OK)
                    return;
            }
            if (preset == null)
                preset = selected != null && !selected.IsAutomatic ? selected : null;

            if (preset == null)
            {
                preset = new SelectionPreset { Id = Guid.NewGuid().ToString("N") };
                _database.SelectionPresets.Insert(0, preset);
            }
            preset.Name = name.Trim();
            preset.Keys = keys;
            preset.IsAutomatic = false;
            preset.UpdatedUtc = DateTime.UtcNow;
            _database.ProtectedKeys = new List<string>(keys);
            _database.ActiveSelectionPresetId = preset.Id;
            _store.Save(_database);
            RefreshSelectionPresets();
            _status.Text = Localization.T("已保存选择方案：") + preset.Name
                + "  ·  " + keys.Count + " " + Localization.T("项");
        }

        private SelectionPreset SaveSelectionPresetInternal(
            string name, IEnumerable<string> keys, bool automatic)
        {
            if (_database.SelectionPresets == null)
                _database.SelectionPresets = new List<SelectionPreset>();
            SelectionPreset preset = new SelectionPreset
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                Keys = keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                IsAutomatic = automatic,
                UpdatedUtc = DateTime.UtcNow
            };
            _database.SelectionPresets.Insert(0, preset);
            if (automatic)
            {
                List<SelectionPreset> stale = _database.SelectionPresets
                    .Where(x => x.IsAutomatic)
                    .OrderByDescending(x => x.UpdatedUtc)
                    .Skip(10)
                    .ToList();
                foreach (SelectionPreset item in stale)
                    _database.SelectionPresets.Remove(item);
            }
            _store.Save(_database);
            return preset;
        }

        private void LoadSelectedPreset(object sender, EventArgs e)
        {
            SelectionPreset preset = SelectedSelectionPreset();
            if (preset == null)
            {
                MessageBox.Show(Localization.T("请先选择一个选择方案。"),
                    "TinyTable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _database.ProtectedKeys = preset.Keys == null
                ? new List<string>()
                : preset.Keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            _database.ActiveSelectionPresetId = preset.Id;
            _store.Save(_database);
            HashSet<string> loaded = new HashSet<string>(
                _database.ProtectedKeys, StringComparer.OrdinalIgnoreCase);
            _refreshingChecks = true;
            foreach (ListViewItem item in _iconList.Items)
                item.Checked = loaded.Contains(((DesktopIcon)item.Tag).Key);
            _refreshingChecks = false;
            _status.Text = Localization.T("已载入选择方案：") + preset.Name
                + "  ·  " + _database.ProtectedKeys.Count + " " + Localization.T("项");
        }

        private void DeleteSelectedPreset(object sender, EventArgs e)
        {
            SelectionPreset preset = SelectedSelectionPreset();
            if (preset == null)
                return;
            if (MessageBox.Show(
                Localization.T("确定删除选择方案“") + preset.Name + "”？\n\n"
                + Localization.T("当前已经勾选的项目不会被清空。"),
                "TinyTable", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question) != DialogResult.OK)
                return;
            _database.SelectionPresets.Remove(preset);
            if (string.Equals(_database.ActiveSelectionPresetId, preset.Id,
                StringComparison.OrdinalIgnoreCase))
                _database.ActiveSelectionPresetId = null;
            _store.Save(_database);
            RefreshSelectionPresets();
            _status.Text = Localization.T("选择方案已删除。");
        }

        private void RefreshSelectionPresets()
        {
            if (_selectionPreset == null)
                return;
            _selectionPreset.BeginUpdate();
            _selectionPreset.Items.Clear();
            _selectionPreset.Items.Add(new SelectionPresetListItem(null));
            IEnumerable<SelectionPreset> presets = (_database.SelectionPresets
                ?? new List<SelectionPreset>())
                .OrderByDescending(x => x.UpdatedUtc);
            foreach (SelectionPreset preset in presets)
                _selectionPreset.Items.Add(new SelectionPresetListItem(preset));
            _selectionPreset.EndUpdate();
            _selectionPreset.SelectedIndex = 0;
            for (int i = 0; i < _selectionPreset.Items.Count; i++)
            {
                SelectionPresetListItem item =
                    (SelectionPresetListItem)_selectionPreset.Items[i];
                if (item.Preset != null && string.Equals(
                    item.Preset.Id, _database.ActiveSelectionPresetId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _selectionPreset.SelectedIndex = i;
                    break;
                }
            }
        }

        private SelectionPreset SelectedSelectionPreset()
        {
            SelectionPresetListItem item =
                _selectionPreset == null ? null
                : _selectionPreset.SelectedItem as SelectionPresetListItem;
            return item == null ? null : item.Preset;
        }

        private void ReplaceProtectedKeys(
            IEnumerable<string> keys, string activePresetId)
        {
            _database.ProtectedKeys = keys
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
            _database.ActiveSelectionPresetId = activePresetId;
            _store.Save(_database);
        }

        private void OnIconItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_refreshingChecks || e.Item == null || e.Item.Tag == null)
                return;
            DesktopIcon icon = (DesktopIcon)e.Item.Tag;
            try
            {
                _selectionList.UpdateKeys(
                    new string[] { icon.Key },
                    e.Item.Checked);
                _database = _store.Load();
                _selectionPreset.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError("更新备份列表失败", ex.Message);
            }
        }

        private void RefreshContextMenu(object sender, EventArgs e)
        {
            try
            {
                ShellContextMenuService.Install();
                _status.Text = "Explorer 右键菜单已刷新。Windows 11 可能显示在“显示更多选项”中。";
            }
            catch (Exception ex)
            {
                ShowError("安装右键菜单失败", ex.Message);
            }
        }

        private void ReloadProtectedList()
        {
            _database = _store.Load();
            RefreshSnapshotList();
            RefreshSelectionPresets();
            _refreshingChecks = true;
            _iconList.Items.Clear();
            _refreshingChecks = false;
            RefreshDesktop(null, EventArgs.Empty);
            _status.Text = "已同步 Explorer 右键菜单修改的备份列表。";
        }

        private void SaveCheckedSnapshot(object sender, EventArgs e)
        {
            List<DesktopIcon> selected = _iconList.CheckedItems
                .Cast<ListViewItem>()
                .Select(x => (DesktopIcon)x.Tag)
                .ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("请先勾选至少一个需要保护位置的桌面项目。",
                    "TinyTable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = PromptForName(
                Localization.T("新建局部快照"),
                BuildManualSnapshotName());
            if (name == null)
                return;

            try
            {
                _icons = _desktop.ReadIcons();
                HashSet<string> selectedKeys = new HashSet<string>(
                    selected.Select(x => x.Key), StringComparer.OrdinalIgnoreCase);
                List<DesktopIcon> freshSelected = _icons
                    .Where(x => selectedKeys.Contains(x.Key)).ToList();
                LayoutSnapshot snapshot = TinyTable.LayoutEngine.Capture(
                    name,
                    _icons,
                    freshSelected,
                    _desktop.GetDesktopSize(),
                    _desktop.GetSpacing());
                _database.Snapshots.Insert(0, snapshot);
                _store.Save(_database);
                RefreshSnapshotList();
                _snapshotList.SelectedIndex = 0;
                _status.Text = "已保存“" + name + "”：保护 "
                    + freshSelected.Count + " 个图标位置。";
            }
            catch (Exception ex)
            {
                ShowError("保存快照失败", ex.Message);
            }
        }

        private void RestoreSelectedSnapshot(object sender, EventArgs e)
        {
            LayoutSnapshot snapshot = SelectedSnapshot();
            if (snapshot == null)
            {
                MessageBox.Show("请先选择右侧的一个快照。", "TinyTable",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_desktop.IsAutoArrangeEnabled())
                {
                    if (Automation.DisableAutoArrangeBeforeRestore)
                        _desktop.DisableAutoArrange();
                }
                if (_desktop.IsAutoArrangeEnabled())
                {
                    MessageBox.Show(
                        "Windows 当前开启了“自动排列图标”，系统会拒绝单独设置图标位置。\n\n"
                        + "请右键桌面 → 查看 → 取消“自动排列图标”，然后重试。",
                        "无法恢复",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                RestorePreferences preferences;
                using (RestoreOptionsDialog options =
                    new RestoreOptionsDialog(_database.RestorePreferences))
                {
                    if (options.ShowDialog(this) != DialogResult.OK)
                        return;
                    preferences = options.SelectedPreferences;
                }
                _database.RestorePreferences = preferences;
                _store.Save(_database);

                List<DesktopIcon> current = _desktop.ReadIcons();
                IconPosition desktopSize = _desktop.GetDesktopSize();
                IconPosition spacing = _desktop.GetSpacing();
                RestorePlan plan = TinyTable.LayoutEngine.BuildPlan(
                    snapshot, current, desktopSize, spacing, preferences);

                List<string> removeMissingAfterRestore;
                if (!HandleMissingItems(
                    snapshot, plan.Missing, out removeMissingAfterRestore))
                    return;

                _database.UndoSnapshot = TinyTable.LayoutEngine.Capture(
                    "恢复前自动备份",
                    current,
                    current,
                    desktopSize,
                    spacing);
                _store.Save(_database);

                if (preferences.Mode != RestoreArrangementMode.CollisionOnly
                    && plan.Overflow.Count > 0
                    && plan.AvailableArrangementColumns == 0)
                {
                    MessageBox.Show(
                        "当前桌面没有一列能完整用于排列普通项目。\n\n"
                        + "TinyTable 不会把项目塞进备份图标之间的零散空格。接下来可以按溢出规则"
                        + "把放不下的普通文件收入一个桌面文件夹。",
                        "没有可用的完整列",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                if (!HandleOverflow(plan.Overflow))
                    return;

                if (plan.Overflow.Count > 0)
                {
                    Thread.Sleep(450);
                    Application.DoEvents();
                    current = _desktop.ReadIcons();
                    plan = TinyTable.LayoutEngine.BuildPlan(
                        snapshot, current, desktopSize, spacing, preferences);
                }

                int applied = _desktop.ApplyPositions(plan.Positions);
                int notApplied = Math.Max(0, plan.Positions.Count - applied);
                if (removeMissingAfterRestore.Count > 0)
                {
                    HashSet<string> removeKeys = new HashSet<string>(
                        removeMissingAfterRestore, StringComparer.OrdinalIgnoreCase);
                    snapshot.Targets.RemoveAll(x => removeKeys.Contains(x.Key));
                    _store.Save(_database);
                    RefreshSnapshotList();
                }
                RefreshDesktop(null, EventArgs.Empty);
                _status.Text = "已恢复“" + snapshot.Name + "”：定位 "
                    + applied + " 个项目；保留空位 " + plan.Missing.Count + " 个"
                    + (notApplied > 0 ? "；仍有 " + notApplied + " 个坐标未能写入。" : "。");

                if (notApplied > 0)
                {
                    MessageBox.Show(
                        "TinyTable 已进行了多轮坐标校正，但仍有 " + notApplied
                        + " 个项目没有到达计划位置。\n\n"
                        + "请确认桌面的“自动排列图标”已关闭，然后再次恢复。",
                        "部分图标未能定位",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                if (plan.Overflow.Any(x => x.IsVirtual))
                {
                    MessageBox.Show(
                        "桌面空间不足，并且部分溢出项目是 Windows 系统图标，"
                        + "无法移动到文件夹。普通文件已按规则处理。",
                        "恢复完成但有提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                List<DesktopIcon> remainingPhysical = plan.Overflow.Where(x =>
                    !x.IsVirtual && (File.Exists(x.Path) || Directory.Exists(x.Path))).ToList();
                if (remainingPhysical.Count > 0)
                {
                    MessageBox.Show(
                        "仍有 " + remainingPhysical.Count
                        + " 个普通项目没有可用位置。它们没有被删除，仍保留在桌面目录中。",
                        "桌面空间仍然不足",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ShowError("恢复失败", ex.Message);
            }
        }

        private bool HandleMissingItems(
            LayoutSnapshot snapshot,
            List<SnapshotItem> missing,
            out List<string> removeAfterRestore)
        {
            removeAfterRestore = new List<string>();
            if (missing.Count == 0)
                return true;
            string names = string.Join("\n", missing.Take(12)
                .Select(x => "• " + x.DisplayName).ToArray());
            if (missing.Count > 12)
                names += "\n…另有 " + (missing.Count - 12) + " 项";
            DialogResult result = MessageBox.Show(
                "以下备份目标已经不在桌面：\n\n" + names
                + "\n\n选择“是”：继续保留恢复目标，原位置留空；"
                + "\n选择“否”：从这个快照的后续恢复中移除，但本次原位置仍留空；"
                + "\n选择“取消”：停止恢复。",
                "发现缺失的备份图标",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                return false;
            if (result == DialogResult.No)
            {
                removeAfterRestore.AddRange(missing.Select(x => x.Key));
            }
            return true;
        }

        private bool HandleOverflow(List<DesktopIcon> overflow)
        {
            List<DesktopIcon> movable = overflow.Where(x =>
                !x.IsVirtual && (File.Exists(x.Path) || Directory.Exists(x.Path))).ToList();
            if (movable.Count == 0)
                return true;

            DialogResult result = MessageBox.Show(
                "桌面可用网格不足，有 " + movable.Count
                + " 个新增或被占位的项目放不下。\n\n"
                + "TinyTable 将在桌面创建“布局溢出”文件夹，并把这些项目移入其中。"
                + "\n这是文件移动操作，是否继续？",
                "桌面空间不足",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);
            if (result != DialogResult.OK)
                return false;

            string desktopFolder =
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string overflowFolder = UniquePath(
                desktopFolder,
                "TinyTable 布局溢出 " + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
                true);
            Directory.CreateDirectory(overflowFolder);
            List<string> errors = new List<string>();
            foreach (DesktopIcon icon in movable)
            {
                try
                {
                    string destination = UniquePath(
                        overflowFolder, Path.GetFileName(icon.Path), Directory.Exists(icon.Path));
                    if (Directory.Exists(icon.Path))
                        Directory.Move(icon.Path, destination);
                    else
                        File.Move(icon.Path, destination);
                }
                catch (Exception ex)
                {
                    errors.Add(icon.DisplayName + "：" + ex.Message);
                }
            }
            if (errors.Count > 0)
            {
                MessageBox.Show(
                    "部分项目无法移动到溢出文件夹：\n\n"
                    + string.Join("\n", errors.Take(8).ToArray()),
                    "溢出处理提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            return true;
        }

        private void UndoLastRestore(object sender, EventArgs e)
        {
            if (_database.UndoSnapshot == null)
            {
                MessageBox.Show("当前没有可撤销的位置恢复记录。", "TinyTable",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                if (_desktop.IsAutoArrangeEnabled())
                {
                    MessageBox.Show("请先关闭桌面的“自动排列图标”。", "TinyTable",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                List<DesktopIcon> current = _desktop.ReadIcons();
                RestorePlan plan = TinyTable.LayoutEngine.BuildPlan(
                    _database.UndoSnapshot,
                    current,
                    _desktop.GetDesktopSize(),
                    _desktop.GetSpacing());
                int applied = _desktop.ApplyPositions(plan.Positions);
                RefreshDesktop(null, EventArgs.Empty);
                _status.Text = "已撤销上次位置恢复，共复位 " + applied + " 个项目。";
            }
            catch (Exception ex)
            {
                ShowError("撤销失败", ex.Message);
            }
        }

        private void DeleteSelectedSnapshot(object sender, EventArgs e)
        {
            LayoutSnapshot snapshot = SelectedSnapshot();
            if (snapshot == null)
                return;
            if (MessageBox.Show(
                "确定删除快照“" + snapshot.Name + "”吗？",
                "TinyTable",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question) != DialogResult.OK)
                return;
            _database.Snapshots.Remove(snapshot);
            _store.Save(_database);
            RefreshSnapshotList();
        }

        private void RefreshSnapshotList()
        {
            _snapshotList.BeginUpdate();
            _snapshotList.Items.Clear();
            foreach (LayoutSnapshot snapshot in _database.Snapshots)
                _snapshotList.Items.Add(new SnapshotListItem(snapshot));
            _snapshotList.EndUpdate();
            _snapshotSummary.Text = _database.Snapshots.Count + " "
                + Localization.T("个局部快照");
        }

        private LayoutSnapshot SelectedSnapshot()
        {
            SnapshotListItem item = _snapshotList.SelectedItem as SnapshotListItem;
            return item == null ? null : item.Snapshot;
        }

        private static string PromptForName(string title, string defaultValue)
        {
            using (Form form = new Form())
            {
                form.Text = title;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ShowInTaskbar = false;
                form.AutoScaleMode = AutoScaleMode.Dpi;
                form.ClientSize = new Size(440, 166);
                form.Font = new Font("Microsoft YaHei UI", 9f);
                form.BackColor = Color.FromArgb(247, 248, 249);

                TableLayoutPanel layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.Padding = new Padding(20, 15, 20, 14);
                layout.ColumnCount = 1;
                layout.RowCount = 4;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25f));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
                form.Controls.Add(layout);

                Label label = new Label();
                label.Text = Localization.T("名称");
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Margin = Padding.Empty;
                layout.Controls.Add(label, 0, 0);

                TextBox text = new TextBox();
                text.Text = defaultValue;
                text.SelectAll();
                text.Dock = DockStyle.Fill;
                text.Margin = new Padding(0, 2, 0, 5);
                layout.Controls.Add(text, 0, 1);

                FlowLayoutPanel buttons = new FlowLayoutPanel();
                buttons.Dock = DockStyle.Fill;
                buttons.FlowDirection = FlowDirection.RightToLeft;
                buttons.WrapContents = false;
                buttons.Margin = Padding.Empty;
                layout.Controls.Add(buttons, 0, 3);

                Button cancel = new Button();
                cancel.Text = Localization.T("取消");
                cancel.DialogResult = DialogResult.Cancel;
                cancel.AutoSize = true;
                cancel.MinimumSize = new Size(82, 30);
                cancel.Margin = new Padding(8, 0, 0, 0);

                Button ok = new Button();
                ok.Text = Localization.T("保存");
                ok.DialogResult = DialogResult.OK;
                ok.AutoSize = true;
                ok.MinimumSize = new Size(82, 30);
                ok.Margin = Padding.Empty;
                buttons.Controls.Add(ok);
                buttons.Controls.Add(cancel);
                form.AcceptButton = ok;
                form.CancelButton = cancel;

                return form.ShowDialog() == DialogResult.OK
                    ? text.Text.Trim() : null;
            }
        }

        private static string UniquePath(string folder, string name, bool directory)
        {
            string candidate = Path.Combine(folder, name);
            if (!File.Exists(candidate) && !Directory.Exists(candidate))
                return candidate;
            string stem = directory ? name : Path.GetFileNameWithoutExtension(name);
            string extension = directory ? string.Empty : Path.GetExtension(name);
            for (int i = 2; i < 10000; i++)
            {
                candidate = Path.Combine(folder, stem + " (" + i + ")" + extension);
                if (!File.Exists(candidate) && !Directory.Exists(candidate))
                    return candidate;
            }
            return Path.Combine(folder, stem + "-" + Guid.NewGuid().ToString("N") + extension);
        }

        private static string ShortPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            string desktop = Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory);
            if (path.StartsWith(desktop, StringComparison.OrdinalIgnoreCase))
                return Localization.T("个人桌面");
            return Localization.T("公共桌面");
        }

        private static void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized
                && Automation.TrayEnabled
                && Automation.MinimizeToTray)
            {
                Hide();
                ShowInTaskbar = false;
                ShowTrayMessage("TinyTable 仍在运行", "双击托盘图标可重新打开。");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowExit
                && !_shutdownSaving
                && e.CloseReason == CloseReason.UserClosing
                && Automation.TrayEnabled
                && Automation.CloseToTray)
            {
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
                ShowTrayMessage("TinyTable 已隐藏到托盘", "右键托盘图标可保存、恢复或退出。");
                return;
            }

            if (!_shutdownSaving && Automation.SaveOnExit)
                RunAutomaticSave("程序退出保存", false);
            SaveWindowBounds();
            try { _store.Save(_database); } catch { }
            if (_trayIcon != null)
                _trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SystemEvents.SessionEnding -= OnSessionEnding;
                SystemEvents.SessionSwitch -= OnSessionSwitch;
                if (_autoSaveTimer != null)
                    _autoSaveTimer.Dispose();
                if (_trayIcon != null)
                {
                    _trayIcon.Visible = false;
                    _trayIcon.Dispose();
                    _trayIcon = null;
                }
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == NativeMethods.WM_TINYTABLE_LIST_CHANGED)
            {
                BeginInvoke(new Action(ReloadProtectedList));
                message.Result = IntPtr.Zero;
                return;
            }
            if (message.Msg == NativeMethods.WM_TINYTABLE_SHOW)
            {
                BeginInvoke(new Action(ShowMainWindow));
                message.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref message);
        }

        private sealed class SnapshotListItem
        {
            public SnapshotListItem(LayoutSnapshot snapshot)
            {
                Snapshot = snapshot;
            }
            public LayoutSnapshot Snapshot { get; private set; }
            public override string ToString()
            {
                return Snapshot.Name + "  ·  " + Snapshot.Targets.Count
                    + " " + Localization.T("项") + "  ·  "
                    + Snapshot.CreatedUtc.ToLocalTime().ToString("MM-dd HH:mm");
            }
        }

        private sealed class SelectionPresetListItem
        {
            public SelectionPresetListItem(SelectionPreset preset)
            {
                Preset = preset;
            }
            public SelectionPreset Preset { get; private set; }
            public override string ToString()
            {
                if (Preset == null)
                    return Localization.T("选择一个已保存方案…");
                string prefix = Preset.IsAutomatic
                    ? Localization.T("自动保留") + " · "
                    : string.Empty;
                int count = Preset.Keys == null ? 0 : Preset.Keys.Count;
                return prefix + Preset.Name + "  (" + count + ")";
            }
        }

    }
}
