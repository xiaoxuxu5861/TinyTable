# TinyTable

[中文](#中文) · [English](#english)

## 中文

TinyTable 是一个轻量的 Windows 桌面图标位置备份工具，专注于“局部恢复”。

### 为什么需要它

[DesktopOK](https://www.softwareok.com/?seite=Freeware/DesktopOK) 很适合保存和恢复整个桌面布局，但全局恢复也可能影响快照之后新增或重新摆放的图标。

TinyTable 是对这种使用方式的补充：你可以只保护指定的图标、文件和文件夹。恢复时，只有这些目标会回到快照位置，其余项目按照你选择的规则避让或重新排列。

TinyTable 与 DesktopOK、SoftwareOK 没有隶属或合作关系。

### 主要功能

- 选择性备份与恢复桌面图标坐标
- 从桌面右键菜单批量加入或移除保护列表
- 快速多选文件夹、快捷方式、图片、文档等类型
- 保存常用选择方案，清空前自动保留恢复记录
- 五种未保护项目处理方式：仅避让、保持顺序、按类型、按名称、按日期
- 缺失目标提示、恢复前自动备份与撤销
- 桌面放不下时先询问，再决定是否移入溢出文件夹
- 托盘常驻、开机启动、自动保存和自动恢复均可选
- 18 种界面语言，切换后立即刷新
- Installer 与 Portable 两种发布方式

### 基本使用

1. 关闭 `桌面右键 → 查看 → 自动排列图标`。“将图标与网格对齐”可以保留。
2. 在 TinyTable 中勾选需要保护的项目，或使用“读取桌面选中项”“快速选择”。
3. 点击“备份勾选项目”创建局部快照。
4. 选择快照并点击“恢复”，再选择未保护项目的处理方式。

TinyTable 只备份图标位置，不备份文件内容。除用户确认的溢出处理外，恢复操作不会移动真实文件。

### 数据位置

- Installer / Portable：`<程序目录>\data\layout-snapshots.json`
- 直接运行开发版：`%LOCALAPPDATA%\TinyTable\layout-snapshots.json`

### 构建

需要 Windows 和 .NET Framework 4.8：

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
powershell -ExecutionPolicy Bypass -File .\build-packages.ps1 -RequireInstaller
```

输出位于 `bin\Release` 和 `dist`。

---

## English

TinyTable is a lightweight Windows utility for selectively backing up and restoring desktop icon positions.

### Why TinyTable

[DesktopOK](https://www.softwareok.com/?seite=Freeware/DesktopOK) is excellent for saving and restoring a complete desktop layout. A full-layout restore, however, may also affect icons that were added or rearranged after the snapshot.

TinyTable complements that workflow by protecting only the icons, files, and folders you choose. During restore, protected items return to their saved coordinates while everything else is left alone or rearranged according to your selected policy.

TinyTable is not affiliated with DesktopOK or SoftwareOK.

### Highlights

- Selective backup and restore of desktop icon coordinates
- Explorer context-menu commands for adding or removing multiple tracked items
- Multi-select by type: folders, shortcuts, images, documents, and more
- Reusable selection sets with automatic recovery records before clearing
- Five policies for unprotected items: collision only, current order, file type, name, or date
- Missing-item prompts, automatic pre-restore backup, and undo
- Confirmation before physical files are moved into an overflow folder
- Optional tray mode, startup, automatic saving, and automatic restore
- 18 UI languages with immediate switching
- Installer and portable distributions

### Quick start

1. Disable `Desktop context menu → View → Auto arrange icons`. “Align icons to grid” may remain enabled.
2. Check the items to protect, or use “Use selected” / “Quick select”.
3. Choose “Save snapshot”.
4. Select a snapshot, choose “Restore”, and pick a policy for unprotected items.

TinyTable stores positions only; it does not back up file contents. Real files are moved only when the user explicitly confirms overflow handling.

### Data locations

- Installer / Portable: `<application folder>\data\layout-snapshots.json`
- Development build: `%LOCALAPPDATA%\TinyTable\layout-snapshots.json`

### Build

Windows and .NET Framework 4.8 are required:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
powershell -ExecutionPolicy Bypass -File .\build-packages.ps1 -RequireInstaller
```

Build artifacts are written to `bin\Release` and `dist`.
