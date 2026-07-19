# TinyTable 更新日志 / Changelog

## 0.1.10 - 2026-07-19

### 中文

本次更新重点改善 Windows 桌面右键菜单的单选、多选与列表同步体验。

- 修复批量加入或移除时偶发的“另一个进程正在使用此文件”错误。
- 右键菜单直接读取 Windows 桌面的当前选中项，避免快捷方式启动参数被误识别为文件。
- 文件匹配保留完整文件名和扩展名，减少同名项目误判。
- 同一批多选操作只执行一次桌面扫描，其余重复辅助进程立即退出。
- 新增轻量选中项读取，只读取稳定标识，不再读取无关的坐标、类型和日期。
- 只有备份列表确实变化时才保存数据并通知主界面。
- 主界面收到右键菜单更新后，仅同步现有项目的勾选状态，不再重新扫描整个桌面。
- 合并短时间内的重复刷新通知，减少列表闪烁和等待时间。
- 数据库写入增加跨进程互斥、唯一临时文件和原子替换，提升并发操作可靠性。
- 修复界面勾选状态与实际备份列表偶尔不同步的问题。

### English

This release focuses on faster and more reliable single- and multi-selection handling from the Windows desktop context menu.

- Fixed intermittent “another process is using this file” errors during bulk add or remove operations.
- Context-menu commands now read the current Windows desktop selection directly, preventing shortcut launch arguments from being mistaken for files.
- File matching preserves complete names and extensions to reduce collisions between similarly named items.
- A multi-selection batch performs only one desktop scan; duplicate helper processes exit immediately.
- Added a lightweight selection reader that skips unrelated position, type, and date metadata.
- Data is saved and the main window is notified only when the protected list actually changes.
- Context-menu updates now refresh existing check states instead of rescanning and rebuilding the complete desktop list.
- Closely spaced refresh notifications are coalesced to reduce flicker and waiting.
- Database writes now use a cross-process mutex, unique temporary files, and atomic replacement for safer concurrent access.
- Fixed occasional differences between visible check states and the stored protected list.
