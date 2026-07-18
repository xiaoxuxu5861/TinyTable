using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TinyTable
{
    public sealed class QuickSelectionDialog : Form
    {
        private readonly CheckedListBox _categories;

        public QuickSelectionDialog(
            IDictionary<string, int> categoryCounts,
            IEnumerable<string> initiallySelected)
        {
            HashSet<string> selected = new HashSet<string>(
                initiallySelected ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            Text = Localization.T("快速选择");
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(390, 420);
            BackColor = Color.FromArgb(247, 248, 249);
            Font = new Font("Microsoft YaHei UI", 9f);
            AutoScaleMode = AutoScaleMode.Dpi;

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(17, 25, 35),
                Padding = new Padding(22, 12, 18, 8)
            };
            Controls.Add(header);

            Label title = new Label
            {
                Text = Localization.T("快速选择"),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 9)
            };
            header.Controls.Add(title);
            Label hint = new Label
            {
                Text = Localization.T("选择一个或多个项目类型"),
                AutoSize = true,
                ForeColor = Color.FromArgb(159, 174, 188),
                Location = new Point(22, 43)
            };
            header.Controls.Add(hint);

            _categories = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                CheckOnClick = true,
                IntegralHeight = false,
                ItemHeight = 29,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(38, 47, 58),
                Padding = new Padding(12, 8, 12, 8)
            };
            foreach (string category in DesktopItemClassifier.CategoryOrder)
            {
                int count;
                categoryCounts.TryGetValue(category, out count);
                CategoryChoice choice = new CategoryChoice(category, count);
                _categories.Items.Add(choice, selected.Contains(category));
            }
            Controls.Add(_categories);
            _categories.BringToFront();

            FlowLayoutPanel footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 54,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(12, 11, 12, 8),
                BackColor = Color.FromArgb(239, 242, 244)
            };
            Controls.Add(footer);

            Button apply = MakeButton(Localization.T("应用选择"), true);
            apply.DialogResult = DialogResult.OK;
            footer.Controls.Add(apply);
            Button cancel = MakeButton(Localization.T("取消"), false);
            cancel.DialogResult = DialogResult.Cancel;
            footer.Controls.Add(cancel);
            Button clear = MakeButton(Localization.T("清空"), false);
            clear.Click += delegate { SetAll(false); };
            footer.Controls.Add(clear);
            Button all = MakeButton(Localization.T("全选"), false);
            all.Click += delegate { SetAll(true); };
            footer.Controls.Add(all);

            AcceptButton = apply;
            CancelButton = cancel;
        }

        public List<string> SelectedCategories
        {
            get
            {
                return _categories.CheckedItems.Cast<CategoryChoice>()
                    .Select(x => x.Category).ToList();
            }
        }

        private void SetAll(bool value)
        {
            for (int i = 0; i < _categories.Items.Count; i++)
                _categories.SetItemChecked(i, value);
        }

        private static Button MakeButton(string text, bool primary)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                Height = 30,
                Margin = new Padding(4, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = primary
                    ? Color.FromArgb(28, 137, 113)
                    : Color.FromArgb(250, 251, 252),
                ForeColor = primary ? Color.White : Color.FromArgb(42, 52, 63)
            };
        }

        private sealed class CategoryChoice
        {
            public CategoryChoice(string category, int count)
            {
                Category = category;
                Count = count;
            }

            public string Category { get; private set; }
            public int Count { get; private set; }

            public override string ToString()
            {
                return Localization.T(Category) + "    " + Count + " "
                    + Localization.T("项");
            }
        }
    }
}
