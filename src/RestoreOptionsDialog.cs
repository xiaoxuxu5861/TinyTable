using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TinyTable
{
    public sealed class RestoreOptionsDialog : Form
    {
        private readonly Dictionary<RestoreArrangementMode, RadioButton> _modes;
        private readonly Dictionary<string, ComboBox> _typeSorts;
        private readonly Panel _typePanel;

        public RestoreOptionsDialog(RestorePreferences current)
        {
            if (current == null)
                current = RestorePreferences.CreateDefault();
            _modes = new Dictionary<RestoreArrangementMode, RadioButton>();
            _typeSorts = new Dictionary<string, ComboBox>(
                StringComparer.OrdinalIgnoreCase);

            Text = "选择其他图标的处理方式";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(760, 690);
            BackColor = Color.FromArgb(243, 246, 248);
            Font = new Font("Microsoft YaHei UI", 9f);
            try
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    typeof(RestoreOptionsDialog).Assembly.Location);
            }
            catch { }

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 82;
            header.BackColor = Color.FromArgb(23, 31, 42);
            Controls.Add(header);

            Label title = new Label();
            title.Text = "恢复目标之外，其余图标怎么处理？";
            title.AutoSize = true;
            title.Font = new Font("Microsoft YaHei UI", 15f, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(24, 15);
            header.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "备份目标始终回到原位置；这里只决定未被保护的其他桌面项目。";
            subtitle.AutoSize = true;
            subtitle.ForeColor = Color.FromArgb(164, 178, 192);
            subtitle.Location = new Point(27, 51);
            header.Controls.Add(subtitle);

            Panel footer = new Panel();
            footer.Dock = DockStyle.Bottom;
            footer.Height = 58;
            footer.BackColor = Color.White;
            Controls.Add(footer);

            Button cancel = new Button();
            cancel.Text = "取消";
            cancel.DialogResult = DialogResult.Cancel;
            cancel.Size = new Size(92, 34);
            cancel.Location = new Point(552, 12);
            footer.Controls.Add(cancel);

            Button restore = new Button();
            restore.Text = "按此策略恢复";
            restore.DialogResult = DialogResult.OK;
            restore.Size = new Size(104, 34);
            restore.Location = new Point(650, 12);
            restore.FlatStyle = FlatStyle.Flat;
            restore.FlatAppearance.BorderSize = 0;
            restore.BackColor = Color.FromArgb(28, 137, 113);
            restore.ForeColor = Color.White;
            restore.Click += OnConfirm;
            footer.Controls.Add(restore);
            AcceptButton = restore;
            CancelButton = cancel;

            Panel body = new Panel();
            body.Dock = DockStyle.Fill;
            body.AutoScroll = true;
            body.Padding = new Padding(22, 16, 22, 18);
            Controls.Add(body);
            body.BringToFront();

            Panel strategyPanel = new Panel();
            strategyPanel.Location = new Point(22, 16);
            strategyPanel.Size = new Size(700, 282);
            strategyPanel.BackColor = Color.White;
            body.Controls.Add(strategyPanel);

            AddMode(strategyPanel, 0, RestoreArrangementMode.CollisionOnly,
                "1  保持不动，只避让冲突",
                "其他图标保持当前位置；只有侵入备份目标位置的项目才会移动。");
            AddMode(strategyPanel, 1, RestoreArrangementMode.CompactCurrentOrder,
                "2  保持当前相对顺序，紧凑排列到新列",
                "按现在从左到右、从上到下的顺序去掉空格，在旁边逐列排列。");
            AddMode(strategyPanel, 2, RestoreArrangementMode.GroupByType,
                "3  按文件类型分组排列",
                "文件夹、快捷方式、图片、文档等分组；每类可单独按名称或日期排序。");
            AddMode(strategyPanel, 3, RestoreArrangementMode.SortByName,
                "4  全部按名称排列",
                "不区分类型，按名称 A-Z 排列到连续的新列。");
            AddMode(strategyPanel, 4, RestoreArrangementMode.SortByDate,
                "5  全部按日期排列",
                "不区分类型，按最后修改时间从新到旧排列。");

            _typePanel = new Panel();
            _typePanel.Location = new Point(22, 310);
            _typePanel.Size = new Size(700, 218);
            _typePanel.BackColor = Color.White;
            body.Controls.Add(_typePanel);

            Label typeTitle = new Label();
            typeTitle.Text = "按类型排序的细则";
            typeTitle.AutoSize = true;
            typeTitle.Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold);
            typeTitle.ForeColor = Color.FromArgb(28, 37, 48);
            typeTitle.Location = new Point(16, 13);
            _typePanel.Controls.Add(typeTitle);

            TableLayoutPanel typeTable = new TableLayoutPanel();
            typeTable.Location = new Point(16, 43);
            typeTable.Size = new Size(668, 162);
            typeTable.ColumnCount = 4;
            typeTable.RowCount = 4;
            typeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            typeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            typeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            typeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _typePanel.Controls.Add(typeTable);

            for (int i = 0; i < DesktopItemClassifier.CategoryOrder.Length; i++)
            {
                string category = DesktopItemClassifier.CategoryOrder[i];
                int row = i % 4;
                int pair = i / 4;
                int labelColumn = pair * 2;
                int comboColumn = labelColumn + 1;

                Label label = new Label();
                label.Text = Localization.T(category);
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.ForeColor = Color.FromArgb(60, 72, 84);
                label.Margin = new Padding(3, 4, 3, 4);
                typeTable.Controls.Add(label, labelColumn, row);

                ComboBox combo = new ComboBox();
                combo.DropDownStyle = ComboBoxStyle.DropDownList;
                combo.Items.Add(Localization.T("按名称 A-Z"));
                combo.Items.Add(Localization.T("按日期 新→旧"));
                combo.SelectedIndex = current.GetTypeSort(category) == IconSortMode.Date
                    ? 1 : 0;
                combo.Dock = DockStyle.Fill;
                combo.Margin = new Padding(3, 6, 14, 6);
                typeTable.Controls.Add(combo, comboColumn, row);
                _typeSorts.Add(category, combo);
            }

            RadioButton selected;
            if (!_modes.TryGetValue(current.Mode, out selected))
                selected = _modes[RestoreArrangementMode.CollisionOnly];
            selected.Checked = true;
            UpdateTypePanelState();
            Localization.Apply(this);
        }

        public RestorePreferences SelectedPreferences { get; private set; }

        private void AddMode(
            Control parent,
            int index,
            RestoreArrangementMode mode,
            string title,
            string description)
        {
            int top = 10 + index * 54;
            RadioButton radio = new RadioButton();
            radio.Text = title;
            radio.AutoSize = true;
            radio.Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Bold);
            radio.ForeColor = Color.FromArgb(30, 40, 51);
            radio.Location = new Point(16, top);
            radio.CheckedChanged += delegate { UpdateTypePanelState(); };
            parent.Controls.Add(radio);
            _modes.Add(mode, radio);

            Label detail = new Label();
            detail.Text = description;
            detail.AutoSize = true;
            detail.ForeColor = Color.FromArgb(105, 117, 130);
            detail.Location = new Point(38, top + 27);
            parent.Controls.Add(detail);
        }

        private void UpdateTypePanelState()
        {
            RadioButton group;
            bool enabled = _modes.TryGetValue(
                RestoreArrangementMode.GroupByType, out group) && group.Checked;
            if (_typePanel != null)
            {
                _typePanel.Enabled = enabled;
                _typePanel.BackColor = enabled
                    ? Color.White
                    : Color.FromArgb(235, 238, 241);
            }
        }

        private void OnConfirm(object sender, EventArgs e)
        {
            RestorePreferences result = new RestorePreferences();
            result.Mode = _modes.First(x => x.Value.Checked).Key;
            foreach (string category in DesktopItemClassifier.CategoryOrder)
            {
                result.TypeSorts.Add(new TypeSortPreference
                {
                    Category = category,
                    SortMode = _typeSorts[category].SelectedIndex == 1
                        ? IconSortMode.Date
                        : IconSortMode.Name
                });
            }
            SelectedPreferences = result;
        }
    }
}
