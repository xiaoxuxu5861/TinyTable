using System;
using System.Drawing;
using System.Windows.Forms;

namespace TinyTable
{
    public sealed class AutomationSettingsDialog : Form
    {
        private readonly CheckBox _trayEnabled;
        private readonly CheckBox _minimizeToTray;
        private readonly CheckBox _closeToTray;
        private readonly CheckBox _startWithWindows;
        private readonly CheckBox _startMinimized;
        private readonly CheckBox _saveWindowPosition;
        private readonly CheckBox _autoSaveEnabled;
        private readonly ComboBox _interval;
        private readonly CheckBox _onlyChanged;
        private readonly ComboBox _retention;
        private readonly CheckBox _saveOnExit;
        private readonly CheckBox _saveOnShutdown;
        private readonly CheckBox _restoreOnProgramStart;
        private readonly CheckBox _restoreOnWindowsStart;
        private readonly CheckBox _saveOnSessionLock;
        private readonly CheckBox _restoreOnSessionUnlock;
        private readonly CheckBox _disableArrangeOnStart;
        private readonly CheckBox _disableArrangeBeforeRestore;
        private readonly ComboBox _namingMode;
        private readonly TextBox _customPrefix;
        private readonly ComboBox _language;
        private readonly string _originalLanguageCode;

        public event EventHandler LanguagePreviewChanged;

        public AutomationSettingsDialog(AutomationSettings current)
        {
            _originalLanguageCode = Localization.LanguageCode;
            if (current == null)
                current = AutomationSettings.CreateDefault();
            Text = "TinyTable 自动化与常驻设置";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(690, 585);
            BackColor = Color.FromArgb(243, 246, 248);
            Font = new Font("Microsoft YaHei UI", 9f);
            try
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    typeof(AutomationSettingsDialog).Assembly.Location);
            }
            catch { }

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 2;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            Controls.Add(root);

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Margin = new Padding(12, 12, 12, 4);
            root.Controls.Add(tabs, 0, 0);

            TabPage resident = NewTab("常驻与启动");
            TabPage autoSave = NewTab("自动保存");
            TabPage restore = NewTab("恢复与命名");
            tabs.TabPages.Add(resident);
            tabs.TabPages.Add(autoSave);
            tabs.TabPages.Add(restore);

            AddSectionTitle(resident, "系统托盘", 20);
            _trayEnabled = AddCheck(resident,
                "启用系统托盘图标（通知区域常驻）", 55, current.TrayEnabled);
            _minimizeToTray = AddCheck(resident,
                "最小化窗口时隐藏到托盘", 90, current.MinimizeToTray);
            _closeToTray = AddCheck(resident,
                "点击关闭按钮时隐藏到托盘，而不是退出", 125, current.CloseToTray);

            AddSectionTitle(resident, "Windows 启动", 180);
            _startWithWindows = AddCheck(resident,
                "登录 Windows 后自动启动 TinyTable", 215, current.StartWithWindows);
            _startMinimized = AddCheck(resident,
                "启动后直接隐藏到托盘", 250, current.StartMinimized);
            _saveWindowPosition = AddCheck(resident,
                "记住主窗口位置和大小", 305, current.SaveWindowPosition);

            Label residentNote = AddNote(resident,
                "关闭托盘功能时，“最小化到托盘”和“关闭到托盘”不会生效。", 355);

            AddSectionTitle(resident, "界面语言", 400);
            _language = AddCombo(resident, 175, 428, 220);
            _language.MaxDropDownItems = 16;
            AddLabel(resident, "界面语言", 48, 434);
            foreach (LanguageInfo language in Localization.SupportedLanguages)
                _language.Items.Add(new LanguageItem(
                    language.NativeName, language.Code));
            SelectLanguage(_language, current.LanguageCode);

            AddSectionTitle(autoSave, "定时保存局部快照", 20);
            _autoSaveEnabled = AddCheck(autoSave,
                "启用自动保存", 55, current.AutoSaveEnabled);
            AddLabel(autoSave, "保存间隔", 48, 101);
            _interval = AddCombo(autoSave, 145, 95, 180);
            _interval.Items.Add(new ValueItem("每 15 分钟", 15));
            _interval.Items.Add(new ValueItem("每小时", 60));
            _interval.Items.Add(new ValueItem("每 6 小时", 360));
            _interval.Items.Add(new ValueItem("每天一次", 1440));
            SelectValue(_interval, current.AutoSaveMinutes);
            _onlyChanged = AddCheck(autoSave,
                "仅当桌面布局发生变化时保存", 140, current.OnlyWhenLayoutChanged);

            AddSectionTitle(autoSave, "触发时机与保留数量", 195);
            _saveOnExit = AddCheck(autoSave,
                "程序真正退出时保存", 230, current.SaveOnExit);
            _saveOnShutdown = AddCheck(autoSave,
                "Windows 关机或注销时保存", 265, current.SaveOnShutdown);
            AddLabel(autoSave, "自动快照最多保留", 48, 316);
            _retention = AddCombo(autoSave, 195, 310, 130);
            _retention.Items.Add(new ValueItem("最后 4 个", 4));
            _retention.Items.Add(new ValueItem("最后 8 个", 8));
            _retention.Items.Add(new ValueItem("最后 16 个", 16));
            _retention.Items.Add(new ValueItem("最后 32 个", 32));
            _retention.Items.Add(new ValueItem("全部保留", 0));
            SelectValue(_retention, current.AutoSaveRetention);
            AddNote(autoSave,
                "自动保存只保护当前备份列表中的项目，手动快照不会被自动清理。", 365);

            AddSectionTitle(restore, "自动恢复", 20);
            _restoreOnProgramStart = AddCheck(restore,
                "每次启动 TinyTable 时恢复最新快照", 55,
                current.RestoreOnProgramStart);
            _restoreOnWindowsStart = AddCheck(restore,
                "仅在 Windows 自动启动 TinyTable 时恢复最新快照", 90,
                current.RestoreOnWindowsStart);
            _disableArrangeOnStart = AddCheck(restore,
                "程序启动时关闭桌面的“自动排列图标”", 125,
                current.DisableAutoArrangeOnStart);
            _disableArrangeBeforeRestore = AddCheck(restore,
                "恢复前自动关闭“自动排列图标”", 160,
                current.DisableAutoArrangeBeforeRestore);

            _saveOnSessionLock = AddCheck(restore,
                "锁屏、切换用户或控制台断开时保存", 205,
                current.SaveOnSessionLock);
            _restoreOnSessionUnlock = AddCheck(restore,
                "解锁、切回用户或控制台连接时恢复", 240,
                current.RestoreOnSessionUnlock);

            AddSectionTitle(restore, "快照命名", 292);
            AddLabel(restore, "自动快照名称", 48, 338);
            _namingMode = AddCombo(restore, 175, 332, 220);
            _namingMode.Items.Add(new ValueItem("日期和时间", 1));
            _namingMode.Items.Add(new ValueItem("屏幕分辨率", 2));
            _namingMode.Items.Add(new ValueItem("桌面工作区大小", 3));
            _namingMode.Items.Add(new ValueItem("自定义前缀", 4));
            SelectValue(_namingMode, (int)current.NamingMode);
            AddLabel(restore, "自定义前缀", 48, 383);
            _customPrefix = new TextBox();
            _customPrefix.Text = current.CustomNamePrefix ?? "TinyTable";
            _customPrefix.Location = new Point(175, 378);
            _customPrefix.Width = 220;
            restore.Controls.Add(_customPrefix);
            AddNote(restore,
                "自动恢复不会未经确认把文件移动到溢出文件夹。", 430);

            _trayEnabled.CheckedChanged += delegate { UpdateEnabledState(); };
            _autoSaveEnabled.CheckedChanged += delegate { UpdateEnabledState(); };
            _namingMode.SelectedIndexChanged += delegate { UpdateEnabledState(); };
            _restoreOnWindowsStart.CheckedChanged += delegate
            {
                if (_restoreOnWindowsStart.Checked)
                    _startWithWindows.Checked = true;
            };
            UpdateEnabledState();

            Panel footer = new Panel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor = Color.White;
            root.Controls.Add(footer, 0, 1);

            Button cancel = new Button();
            cancel.Text = "取消";
            cancel.DialogResult = DialogResult.Cancel;
            cancel.Size = new Size(88, 34);
            cancel.Location = new Point(492, 12);
            footer.Controls.Add(cancel);

            Button save = new Button();
            save.Text = "保存设置";
            save.DialogResult = DialogResult.OK;
            save.Size = new Size(92, 34);
            save.Location = new Point(586, 12);
            save.FlatStyle = FlatStyle.Flat;
            save.FlatAppearance.BorderSize = 0;
            save.BackColor = Color.FromArgb(28, 137, 113);
            save.ForeColor = Color.White;
            save.Click += OnSave;
            footer.Controls.Add(save);
            AcceptButton = save;
            CancelButton = cancel;
            Localization.Apply(this);
            _language.SelectedIndexChanged += OnLanguageSelectionChanged;
        }

        public AutomationSettings SelectedSettings { get; private set; }

        private void OnLanguageSelectionChanged(object sender, EventArgs e)
        {
            LanguageItem selected = _language.SelectedItem as LanguageItem;
            if (selected == null)
                return;
            Localization.SetLanguage(selected.Code);
            Localization.Apply(this);
            RefreshValueCombo(_interval);
            RefreshValueCombo(_retention);
            RefreshValueCombo(_namingMode);
            EventHandler handler = LanguagePreviewChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private static void RefreshValueCombo(ComboBox combo)
        {
            int selected = combo.SelectedIndex;
            combo.SelectedIndex = -1;
            combo.SelectedIndex = selected;
            combo.Refresh();
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (_startMinimized.Checked && !_trayEnabled.Checked)
            {
                MessageBox.Show(
                    "启动后隐藏到托盘需要先启用系统托盘图标。",
                    "TinyTable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.None;
                return;
            }
            AutomationSettings result = new AutomationSettings();
            result.TrayEnabled = _trayEnabled.Checked;
            result.MinimizeToTray = _minimizeToTray.Checked;
            result.CloseToTray = _closeToTray.Checked;
            result.StartWithWindows = _startWithWindows.Checked;
            result.StartMinimized = _startMinimized.Checked;
            result.SaveWindowPosition = _saveWindowPosition.Checked;
            result.AutoSaveEnabled = _autoSaveEnabled.Checked;
            result.AutoSaveMinutes = ((ValueItem)_interval.SelectedItem).Value;
            result.OnlyWhenLayoutChanged = _onlyChanged.Checked;
            result.AutoSaveRetention = ((ValueItem)_retention.SelectedItem).Value;
            result.SaveOnExit = _saveOnExit.Checked;
            result.SaveOnShutdown = _saveOnShutdown.Checked;
            result.RestoreOnProgramStart = _restoreOnProgramStart.Checked;
            result.RestoreOnWindowsStart = _restoreOnWindowsStart.Checked;
            result.SaveOnSessionLock = _saveOnSessionLock.Checked;
            result.RestoreOnSessionUnlock = _restoreOnSessionUnlock.Checked;
            result.DisableAutoArrangeOnStart = _disableArrangeOnStart.Checked;
            result.DisableAutoArrangeBeforeRestore = _disableArrangeBeforeRestore.Checked;
            result.NamingMode = (SnapshotNamingMode)
                ((ValueItem)_namingMode.SelectedItem).Value;
            result.CustomNamePrefix = string.IsNullOrWhiteSpace(_customPrefix.Text)
                ? "TinyTable" : _customPrefix.Text.Trim();
            result.LanguageCode = ((LanguageItem)_language.SelectedItem).Code;
            SelectedSettings = result;
        }

        private void UpdateEnabledState()
        {
            _minimizeToTray.Enabled = _trayEnabled.Checked;
            _closeToTray.Enabled = _trayEnabled.Checked;
            _startMinimized.Enabled = _trayEnabled.Checked;
            _interval.Enabled = _autoSaveEnabled.Checked;
            _onlyChanged.Enabled = _autoSaveEnabled.Checked;
            _customPrefix.Enabled = _namingMode.SelectedItem is ValueItem
                && ((ValueItem)_namingMode.SelectedItem).Value == 4;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                Localization.SetLanguage(_originalLanguageCode);
                EventHandler handler = LanguagePreviewChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            base.OnFormClosed(e);
        }

        private static TabPage NewTab(string text)
        {
            TabPage page = new TabPage(text);
            page.BackColor = Color.White;
            page.Padding = new Padding(12);
            return page;
        }

        private static void AddSectionTitle(Control parent, string text, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Font = new Font("Microsoft YaHei UI", 10.5f, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(28, 37, 48);
            label.Location = new Point(24, top);
            parent.Controls.Add(label);
        }

        private static CheckBox AddCheck(
            Control parent, string text, int top, bool value)
        {
            CheckBox check = new CheckBox();
            check.Text = text;
            check.Checked = value;
            check.AutoSize = true;
            check.Location = new Point(48, top);
            parent.Controls.Add(check);
            return check;
        }

        private static Label AddNote(Control parent, string text, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.ForeColor = Color.FromArgb(104, 117, 130);
            label.Location = new Point(48, top);
            parent.Controls.Add(label);
            return label;
        }

        private static void AddLabel(Control parent, string text, int left, int top)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Location = new Point(left, top);
            parent.Controls.Add(label);
        }

        private static ComboBox AddCombo(
            Control parent, int left, int top, int width)
        {
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Location = new Point(left, top);
            combo.Width = width;
            parent.Controls.Add(combo);
            return combo;
        }

        private static void SelectValue(ComboBox combo, int value)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (((ValueItem)combo.Items[i]).Value == value)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            combo.SelectedIndex = 0;
        }

        private static void SelectLanguage(ComboBox combo, string code)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                LanguageItem item = (LanguageItem)combo.Items[i];
                if (string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            combo.SelectedIndex = 0;
        }

        private sealed class ValueItem
        {
            public ValueItem(string text, int value)
            {
                Text = text;
                Value = value;
            }
            public string Text { get; private set; }
            public int Value { get; private set; }
            public override string ToString() { return Localization.T(Text); }
        }

        private sealed class LanguageItem
        {
            public LanguageItem(string text, string code)
            {
                Text = text;
                Code = code;
            }
            public string Text { get; private set; }
            public string Code { get; private set; }
            public override string ToString() { return Text; }
        }
    }
}
