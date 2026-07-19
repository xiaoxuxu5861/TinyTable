using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TinyTable
{
    public sealed class LanguageInfo
    {
        public LanguageInfo(string code, string nativeName)
        {
            Code = code;
            NativeName = nativeName;
        }
        public string Code { get; private set; }
        public string NativeName { get; private set; }
    }

    public static partial class Localization
    {
        private sealed class TextSet
        {
            public string English;
            public string Traditional;
        }

        private sealed class SourceText
        {
            public string Value;
        }

        private static readonly Dictionary<string, TextSet> Texts =
            new Dictionary<string, TextSet>(StringComparer.Ordinal);
        private static readonly Dictionary<string, Dictionary<string, string>>
            ExtraTexts = new Dictionary<string, Dictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase);
        private static readonly List<LanguageInfo> Languages =
            new List<LanguageInfo>();
        private static readonly ConditionalWeakTable<Control, SourceText>
            ControlSources = new ConditionalWeakTable<Control, SourceText>();
        private static readonly ConditionalWeakTable<ColumnHeader, SourceText>
            ColumnSources = new ConditionalWeakTable<ColumnHeader, SourceText>();

        static Localization()
        {
            LanguageCode = "zh-CN";
            Add("TinyTable · 局部桌面快照", "TinyTable · Selective Desktop Snapshots", "TinyTable · 局部桌面快照");
            Add("局部桌面快照", "Selective Desktop Snapshots", "局部桌面快照");
            Add("只恢复你指定的图标位置；新增项目会被安全让位，不会凭空消失。",
                "Restore only selected icon positions; new items are safely relocated.",
                "只還原你指定的圖示位置；新增項目會被安全讓位，不會憑空消失。");
            Add("局部桌面备份  ·  只保护你选择的图标位置",
                "Selective desktop backup  ·  Protect only the icons you choose",
                "局部桌面備份  ·  只保護你選擇的圖示位置");
            Add("选择方案", "Selection set", "選擇方案");
            Add("载入", "Load", "載入");
            Add("保存当前选择", "Save selection", "儲存目前選擇");
            Add("保存方案", "Save set", "儲存方案");
            Add("删除方案", "Delete set", "刪除方案");
            Add("选择一个已保存方案…", "Choose a saved set…", "選擇一個已儲存方案…");
            Add("撤销恢复", "Undo restore", "復原還原");
            Add("选择一个记录，恢复其中指定图标的位置",
                "Choose a record to restore its selected icon positions",
                "選擇一筆記錄，還原其中指定圖示的位置");
            Add("保存选择方案", "Save selection set", "儲存選擇方案");
            Add("名称", "Name", "名稱");
            Add("新建局部快照", "New selective snapshot", "新增局部快照");
            Add("保存", "Save", "儲存");
            Add("清空前", "Before clear", "清除前");
            Add("自动保留", "Auto recovery", "自動保留");
            Add("当前选择已经是空的。", "The current selection is already empty.", "目前選擇已經是空的。");
            Add("Windows 当前没有选中的桌面图标。",
                "No desktop icons are currently selected in Windows.",
                "Windows 目前沒有選取的桌面圖示。");
            Add("选择已清空；可从方案中恢复：", "Selection cleared; recover it from: ", "選擇已清除；可從方案中恢復：");
            Add("当前没有勾选项目，不能保存选择方案。", "No items are checked, so the selection set cannot be saved.", "目前沒有勾選項目，無法儲存選擇方案。");
            Add("已有同名方案，是否用当前选择覆盖它？", "A set with this name exists. Replace it with the current selection?", "已有同名方案，是否以目前選擇覆蓋？");
            Add("已保存选择方案：", "Selection set saved: ", "已儲存選擇方案：");
            Add("请先选择一个选择方案。", "Choose a selection set first.", "請先選擇一個選擇方案。");
            Add("已载入选择方案：", "Selection set loaded: ", "已載入選擇方案：");
            Add("确定删除选择方案“", "Delete selection set “", "確定刪除選擇方案「");
            Add("当前已经勾选的项目不会被清空。", "Items that are currently checked will remain checked.", "目前已勾選的項目不會被清除。");
            Add("选择方案已删除。", "Selection set deleted.", "選擇方案已刪除。");
            Add("刷新桌面", "Refresh", "重新整理桌面");
            Add("读取桌面选中项", "Use selected", "讀取桌面選取項目");
            Add("全选", "All", "全選");
            Add("清空", "Clear", "清除");
            Add("快速选择 ▾", "Quick select ▾", "快速選擇 ▾");
            Add("快速选择", "Quick select", "快速選擇");
            Add("选择一个或多个项目类型", "Choose one or more item types", "選擇一個或多個項目類型");
            Add("应用选择", "Apply selection", "套用選擇");
            Add("种类型", "types", "種類型");
            Add("更多 ▾", "More ▾", "更多 ▾");
            Add("快照操作 ▾", "Snapshot ▾", "快照操作 ▾");
            Add("载入选择方案", "Load selection set", "載入選擇方案");
            Add("已快速选择", "Quick-selected", "已快速選擇");
            Add("备份勾选项目", "Save snapshot", "備份勾選項目");
            Add("刷新右键菜单", "Context menu", "重新整理右鍵選單");
            Add("恢复", "Restore", "還原");
            Add("删除快照", "Delete snapshot", "刪除快照");
            Add("撤销上次位置恢复", "Undo last restore", "復原上次位置還原");
            Add("设置", "Settings", "設定");
            Add("打开 TinyTable", "Open TinyTable", "開啟 TinyTable");
            Add("立即自动保存", "Save snapshot now", "立即自動儲存");
            Add("恢复最新快照", "Restore latest snapshot", "還原最新快照");
            Add("启用自动保存", "Enable auto-save", "啟用自動儲存");
            Add("自动化与常驻设置…", "Automation & tray settings…", "自動化與常駐設定…");
            Add("退出 TinyTable", "Exit TinyTable", "結束 TinyTable");
            Add("加入 TinyTable 备份列表", "Add to TinyTable tracking list", "加入 TinyTable 備份清單");
            Add("从 TinyTable 备份列表移除", "Remove from TinyTable tracking list", "從 TinyTable 備份清單移除");
            Add("备份", "Track", "備份");
            Add("桌面项目", "Desktop item", "桌面項目");
            Add("类型", "Type", "類型");
            Add("当前位置", "Current position", "目前位置");
            Add("来源", "Source", "來源");
            Add("显示排序", "Display order", "顯示排序");
            Add("按桌面位置", "Desktop position", "依桌面位置");
            Add("按文件类型", "File type", "依檔案類型");
            Add("按名称 A-Z", "Name A-Z", "依名稱 A-Z");
            Add("按日期 新→旧", "Date: newest first", "依日期：新到舊");
            Add("文件夹", "Folders", "資料夾");
            Add("快捷方式", "Shortcuts", "捷徑");
            Add("图片", "Images", "圖片");
            Add("文档", "Documents", "文件");
            Add("压缩包", "Archives", "壓縮檔");
            Add("音视频", "Audio & video", "音訊與影片");
            Add("其他", "Other", "其他");
            Add("系统图标", "System icons", "系統圖示");
            Add("个桌面项目", "desktop items", "個桌面項目");
            Add("个局部快照", "selective snapshots", "個局部快照");
            Add("项", "items", "項");
            Add("个人桌面", "Personal desktop", "個人桌面");
            Add("公共桌面", "Public desktop", "公用桌面");

            Add("选择其他图标的处理方式", "Choose how to arrange other icons", "選擇其他圖示的處理方式");
            Add("恢复目标之外，其余图标怎么处理？",
                "How should untracked icons be arranged?",
                "還原目標之外，其餘圖示如何處理？");
            Add("备份目标始终回到原位置；这里只决定未被保护的其他桌面项目。",
                "Tracked targets always return to their saved positions. This controls all other desktop items.",
                "備份目標一律回到原位置；這裡只決定未受保護的其他桌面項目。");
            Add("1  保持不动，只避让冲突", "1  Keep in place; move collisions only", "1  保持不動，只避讓衝突");
            Add("2  保持当前相对顺序，紧凑排列到新列", "2  Keep current order; pack into new columns", "2  保持目前相對順序，緊密排列到新欄");
            Add("3  按文件类型分组排列", "3  Group by file type", "3  依檔案類型分組排列");
            Add("4  全部按名称排列", "4  Sort all by name", "4  全部依名稱排列");
            Add("5  全部按日期排列", "5  Sort all by date", "5  全部依日期排列");
            Add("其他图标保持当前位置；只有侵入备份目标位置的项目才会移动。",
                "Keep other icons in place; only items colliding with tracked positions are moved.",
                "其他圖示保持目前位置；只有侵入備份目標位置的項目才會移動。");
            Add("按现在从左到右、从上到下的顺序去掉空格，在旁边逐列排列。",
                "Remove gaps while preserving the current left-to-right, top-to-bottom order, then pack into adjacent columns.",
                "依目前由左至右、由上至下的順序移除空格，並在旁邊逐欄排列。");
            Add("文件夹、快捷方式、图片、文档等分组；每类可单独按名称或日期排序。",
                "Group folders, shortcuts, images, documents, and more; choose name or date sorting for each type.",
                "將資料夾、捷徑、圖片、文件等分組；每類可個別選擇依名稱或日期排序。");
            Add("不区分类型，按名称 A-Z 排列到连续的新列。",
                "Ignore file types and pack all items into new columns by name A-Z.",
                "不區分類型，依名稱 A-Z 排列到連續的新欄。");
            Add("不区分类型，按最后修改时间从新到旧排列。",
                "Ignore file types and sort by last modified time, newest first.",
                "不區分類型，依最後修改時間由新到舊排列。");
            Add("按类型排序的细则", "Per-type sorting", "依類型排序的細則");
            Add("取消", "Cancel", "取消");
            Add("按此策略恢复", "Restore with this strategy", "依此策略還原");

            Add("TinyTable 自动化与常驻设置", "TinyTable Automation & Tray Settings", "TinyTable 自動化與常駐設定");
            Add("常驻与启动", "Tray & startup", "常駐與啟動");
            Add("自动保存", "Auto-save", "自動儲存");
            Add("恢复与命名", "Restore & naming", "還原與命名");
            Add("系统托盘", "System tray", "系統匣");
            Add("启用系统托盘图标（通知区域常驻）", "Enable system tray icon", "啟用系統匣圖示（通知區域常駐）");
            Add("最小化窗口时隐藏到托盘", "Hide to tray when minimized", "視窗最小化時隱藏到系統匣");
            Add("点击关闭按钮时隐藏到托盘，而不是退出", "Hide to tray when the close button is clicked", "按關閉按鈕時隱藏到系統匣，而不是結束");
            Add("Windows 启动", "Windows startup", "Windows 啟動");
            Add("登录 Windows 后自动启动 TinyTable", "Start TinyTable after Windows sign-in", "登入 Windows 後自動啟動 TinyTable");
            Add("启动后直接隐藏到托盘", "Start hidden in the tray", "啟動後直接隱藏到系統匣");
            Add("记住主窗口位置和大小", "Remember main window position and size", "記住主視窗位置和大小");
            Add("关闭托盘功能时，“最小化到托盘”和“关闭到托盘”不会生效。",
                "Minimize-to-tray and close-to-tray require the system tray icon.",
                "關閉系統匣功能時，「最小化到系統匣」與「關閉到系統匣」不會生效。");
            Add("定时保存局部快照", "Scheduled selective snapshots", "定時儲存局部快照");
            Add("启用自动保存", "Enable auto-save", "啟用自動儲存");
            Add("保存间隔", "Save interval", "儲存間隔");
            Add("仅当桌面布局发生变化时保存", "Save only when the desktop layout changes", "僅當桌面配置發生變化時儲存");
            Add("触发时机与保留数量", "Triggers & retention", "觸發時機與保留數量");
            Add("程序真正退出时保存", "Save when the program actually exits", "程式真正結束時儲存");
            Add("Windows 关机或注销时保存", "Save on Windows shutdown or sign-out", "Windows 關機或登出時儲存");
            Add("自动快照最多保留", "Keep auto-snapshots", "自動快照最多保留");
            Add("自动保存只保护当前备份列表中的项目，手动快照不会被自动清理。",
                "Auto-save protects only items in the tracking list. Manual snapshots are never pruned automatically.",
                "自動儲存只保護目前備份清單中的項目，手動快照不會被自動清理。");
            Add("自动恢复", "Automatic restore", "自動還原");
            Add("每次启动 TinyTable 时恢复最新快照", "Restore the latest snapshot whenever TinyTable starts", "每次啟動 TinyTable 時還原最新快照");
            Add("仅在 Windows 自动启动 TinyTable 时恢复最新快照", "Restore only when TinyTable starts with Windows", "僅在 Windows 自動啟動 TinyTable 時還原最新快照");
            Add("程序启动时关闭桌面的“自动排列图标”", "Disable desktop auto-arrange when the program starts", "程式啟動時關閉桌面的「自動排列圖示」");
            Add("恢复前自动关闭“自动排列图标”", "Disable auto-arrange before restoring", "還原前自動關閉「自動排列圖示」");
            Add("锁屏、切换用户或控制台断开时保存", "Save on lock, user switch, or console disconnect", "鎖定、切換使用者或主控台中斷時儲存");
            Add("解锁、切回用户或控制台连接时恢复", "Restore on unlock, user return, or console reconnect", "解鎖、切回使用者或主控台連線時還原");
            Add("快照命名", "Snapshot naming", "快照命名");
            Add("自动快照名称", "Auto-snapshot name", "自動快照名稱");
            Add("自定义前缀", "Custom prefix", "自訂前綴");
            Add("自动恢复不会未经确认把文件移动到溢出文件夹。",
                "Automatic restore never moves files into an overflow folder without confirmation.",
                "自動還原不會未經確認就將檔案移入溢出資料夾。");
            Add("保存设置", "Save settings", "儲存設定");
            Add("界面语言", "Interface language", "介面語言");
            Add("界面语言已切换。", "Interface language changed.", "介面語言已切換。");
            Add("语言设置已立即应用。", "Language settings applied immediately.", "語言設定已立即套用。");
            Add("自动化与常驻设置已保存。", "Automation and tray settings saved.", "自動化與常駐設定已儲存。");
            Add("每 15 分钟", "Every 15 minutes", "每 15 分鐘");
            Add("每小时", "Every hour", "每小時");
            Add("每 6 小时", "Every 6 hours", "每 6 小時");
            Add("每天一次", "Once a day", "每天一次");
            Add("最后 4 个", "Last 4", "最後 4 個");
            Add("最后 8 个", "Last 8", "最後 8 個");
            Add("最后 16 个", "Last 16", "最後 16 個");
            Add("最后 32 个", "Last 32", "最後 32 個");
            Add("全部保留", "Keep all", "全部保留");
            Add("日期和时间", "Date and time", "日期和時間");
            Add("屏幕分辨率", "Screen resolution", "螢幕解析度");
            Add("桌面工作区大小", "Desktop work area size", "桌面工作區大小");
            InitializeLanguagePacks();
        }

        public static string LanguageCode { get; private set; }

        public static void SetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code))
                code = "zh-CN";
            LanguageCode = IsSupportedLanguage(code) ? code : "en-US";
        }

        public static IList<LanguageInfo> SupportedLanguages
        {
            get { return Languages.AsReadOnly(); }
        }

        public static string GetBestLanguageCode(CultureInfo culture)
        {
            if (culture == null)
                return "en-US";
            string name = culture.Name ?? string.Empty;
            string language = culture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (language == "zh")
            {
                string upper = name.ToUpperInvariant();
                return upper.Contains("TW") || upper.Contains("HK")
                    || upper.Contains("MO") || upper.Contains("HANT")
                    ? "zh-TW" : "zh-CN";
            }
            switch (language)
            {
                case "de": return "de-DE";
                case "fr": return "fr-FR";
                case "es": return "es-ES";
                case "it": return "it-IT";
                case "pt": return "pt-BR";
                case "nl": return "nl-NL";
                case "pl": return "pl-PL";
                case "ru": return "ru-RU";
                case "uk": return "uk-UA";
                case "tr": return "tr-TR";
                case "ja": return "ja-JP";
                case "ko": return "ko-KR";
                case "vi": return "vi-VN";
                case "id": return "id-ID";
                case "cs": return "cs-CZ";
                case "en": return "en-US";
                default: return "en-US";
            }
        }

        public static string T(string simplifiedChinese)
        {
            if (LanguageCode == "zh-CN" || string.IsNullOrEmpty(simplifiedChinese))
                return simplifiedChinese;
            TextSet set;
            if (!Texts.TryGetValue(simplifiedChinese, out set))
                return simplifiedChinese;
            if (LanguageCode == "zh-TW")
                return set.Traditional;
            if (LanguageCode == "en-US")
                return set.English;
            Dictionary<string, string> pack;
            string translated;
            if (ExtraTexts.TryGetValue(LanguageCode, out pack)
                && pack.TryGetValue(simplifiedChinese, out translated))
                return translated;
            return set.English;
        }

        public static void Apply(Control root)
        {
            if (root == null)
                return;
            if (!(root is ComboBox))
            {
                SourceText controlSource = ControlSources.GetValue(
                    root, delegate(Control control)
                    {
                        return new SourceText { Value = control.Text };
                    });
                root.Text = T(controlSource.Value);
            }
            ListView list = root as ListView;
            if (list != null)
            {
                foreach (ColumnHeader column in list.Columns)
                {
                    SourceText columnSource = ColumnSources.GetValue(
                        column, delegate(ColumnHeader header)
                        {
                            return new SourceText { Value = header.Text };
                        });
                    column.Text = T(columnSource.Value);
                }
            }
            foreach (Control child in root.Controls)
                Apply(child);
        }

        private static void Add(string chinese, string english, string traditional)
        {
            Texts[chinese] = new TextSet
            {
                English = english,
                Traditional = traditional
            };
        }

        private static bool IsSupportedLanguage(string code)
        {
            foreach (LanguageInfo language in Languages)
            {
                if (string.Equals(language.Code, code,
                    StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
