using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyTable
{
    public static class LayoutEngine
    {
        public static LayoutSnapshot Capture(
            string name,
            IList<DesktopIcon> allIcons,
            IList<DesktopIcon> selectedIcons,
            IconPosition desktopSize,
            IconPosition spacing)
        {
            LayoutSnapshot snapshot = new LayoutSnapshot();
            snapshot.Id = Guid.NewGuid().ToString("N");
            snapshot.Name = name;
            snapshot.CreatedUtc = DateTime.UtcNow;
            snapshot.DesktopWidth = desktopSize.X;
            snapshot.DesktopHeight = desktopSize.Y;
            snapshot.SpacingX = spacing.X;
            snapshot.SpacingY = spacing.Y;
            foreach (DesktopIcon icon in allIcons)
                snapshot.Baseline.Add(SnapshotItem.FromIcon(icon));
            foreach (DesktopIcon icon in selectedIcons)
                snapshot.Targets.Add(SnapshotItem.FromIcon(icon));
            return snapshot;
        }

        public static RestorePlan BuildPlan(
            LayoutSnapshot snapshot,
            IList<DesktopIcon> currentIcons,
            IconPosition currentDesktopSize,
            IconPosition currentSpacing)
        {
            return BuildPlan(
                snapshot,
                currentIcons,
                currentDesktopSize,
                currentSpacing,
                RestorePreferences.CreateDefault());
        }

        public static RestorePlan BuildPlan(
            LayoutSnapshot snapshot,
            IList<DesktopIcon> currentIcons,
            IconPosition currentDesktopSize,
            IconPosition currentSpacing,
            RestorePreferences preferences)
        {
            if (preferences == null)
                preferences = RestorePreferences.CreateDefault();
            RestorePlan plan = new RestorePlan();
            int spacingX = currentSpacing.X > 20
                ? currentSpacing.X : Math.Max(64, snapshot.SpacingX);
            int spacingY = currentSpacing.Y > 20
                ? currentSpacing.Y : Math.Max(80, snapshot.SpacingY);
            int originX = GetOrigin(snapshot.Baseline.Select(x => x.X), spacingX);
            int originY = GetOrigin(snapshot.Baseline.Select(x => x.Y), spacingY);
            int columns = Math.Max(1, (currentDesktopSize.X - originX) / spacingX);
            int rows = Math.Max(1, (currentDesktopSize.Y - originY) / spacingY);

            Dictionary<string, DesktopIcon> currentByKey =
                new Dictionary<string, DesktopIcon>(StringComparer.OrdinalIgnoreCase);
            foreach (DesktopIcon icon in currentIcons)
            {
                if (!currentByKey.ContainsKey(icon.Key))
                    currentByKey.Add(icon.Key, icon);
            }
            HashSet<string> matchedTargetKeys =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> occupied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<DesktopIcon> relocate = new List<DesktopIcon>();
            int maxTargetColumn = -1;

            foreach (SnapshotItem target in snapshot.Targets)
            {
                DesktopIcon current = FindCurrent(
                    target, currentIcons, currentByKey, matchedTargetKeys);
                Cell targetCell = ToCell(
                    target.X, target.Y, originX, originY, spacingX, spacingY);
                occupied.Add(targetCell.Key);
                maxTargetColumn = Math.Max(maxTargetColumn, targetCell.Column);
                if (current == null)
                {
                    plan.Missing.Add(target);
                    plan.ReservedEmpty.Add(new IconPosition(target.X, target.Y));
                }
                else
                {
                    matchedTargetKeys.Add(current.Key);
                    plan.Positions[current.Key] = new IconPosition(target.X, target.Y);
                }
            }

            foreach (DesktopIcon icon in currentIcons)
            {
                if (matchedTargetKeys.Contains(icon.Key))
                    continue;
                Cell currentCell = ToCell(
                    icon.X, icon.Y, originX, originY, spacingX, spacingY);
                EnsureMetadata(icon);

                if (preferences.Mode == RestoreArrangementMode.CollisionOnly)
                {
                    if (occupied.Contains(currentCell.Key))
                        relocate.Add(icon);
                    else
                        occupied.Add(currentCell.Key);
                    continue;
                }
                relocate.Add(icon);
            }

            int startColumn = Math.Max(0, maxTargetColumn + 1);
            plan.TotalColumns = columns;
            relocate = OrderIcons(
                relocate,
                preferences,
                originX,
                originY,
                spacingX,
                spacingY);

            List<Cell> freeCells;
            if (preferences.Mode == RestoreArrangementMode.CollisionOnly)
            {
                plan.ArrangementStartColumn = startColumn;
                freeCells = BuildFreeCells(
                    occupied, startColumn, columns, rows, true);
            }
            else
            {
                int availableColumns;
                int firstColumn;
                freeCells = BuildWholeFreeColumnCells(
                    occupied,
                    maxTargetColumn,
                    columns,
                    rows,
                    out availableColumns,
                    out firstColumn);
                plan.AvailableArrangementColumns = availableColumns;
                plan.ArrangementStartColumn = firstColumn;
            }
            int freeIndex = 0;
            string previousCategory = null;
            foreach (DesktopIcon icon in relocate)
            {
                if (preferences.Mode == RestoreArrangementMode.GroupByType
                    && previousCategory != null
                    && !string.Equals(previousCategory, icon.Category,
                        StringComparison.OrdinalIgnoreCase))
                {
                    // A visual group must start at the top of a fresh column.
                    // Compacting two types into one column makes the desktop
                    // look interleaved even when the list order is correct.
                    while (freeIndex < freeCells.Count
                        && freeCells[freeIndex].Row != 0)
                        freeIndex++;
                }
                if (freeIndex >= freeCells.Count)
                {
                    plan.Overflow.Add(icon);
                    previousCategory = icon.Category;
                    continue;
                }
                Cell cell = freeCells[freeIndex++];
                occupied.Add(cell.Key);
                plan.Positions[icon.Key] = new IconPosition(
                    originX + cell.Column * spacingX,
                    originY + cell.Row * spacingY);
                plan.ArrangedItemCount++;
                previousCategory = icon.Category;
            }
            return plan;
        }

        private static List<DesktopIcon> OrderIcons(
            List<DesktopIcon> icons,
            RestorePreferences preferences,
            int originX,
            int originY,
            int spacingX,
            int spacingY)
        {
            if (preferences.Mode == RestoreArrangementMode.CompactCurrentOrder
                || preferences.Mode == RestoreArrangementMode.CollisionOnly)
            {
                return icons.OrderBy(x => ToCell(
                        x.X, x.Y, originX, originY, spacingX, spacingY).Column)
                    .ThenBy(x => ToCell(
                        x.X, x.Y, originX, originY, spacingX, spacingY).Row)
                    .ToList();
            }

            if (preferences.Mode == RestoreArrangementMode.SortByName)
                return SortByName(icons);
            if (preferences.Mode == RestoreArrangementMode.SortByDate)
                return SortByDate(icons);
            if (preferences.Mode == RestoreArrangementMode.GroupByType)
            {
                List<DesktopIcon> result = new List<DesktopIcon>();
                HashSet<DesktopIcon> added = new HashSet<DesktopIcon>();
                foreach (string category in DesktopItemClassifier.CategoryOrder)
                {
                    List<DesktopIcon> group = icons
                        .Where(x => string.Equals(
                            x.Category, category, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    group = preferences.GetTypeSort(category) == IconSortMode.Date
                        ? SortByDate(group)
                        : SortByName(group);
                    result.AddRange(group);
                    foreach (DesktopIcon icon in group)
                        added.Add(icon);
                }
                result.AddRange(SortByName(icons.Where(x => !added.Contains(x)).ToList()));
                return result;
            }
            return icons;
        }

        private static List<DesktopIcon> SortByName(List<DesktopIcon> icons)
        {
            return icons.OrderBy(
                x => x.DisplayName,
                StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private static List<DesktopIcon> SortByDate(List<DesktopIcon> icons)
        {
            return icons
                .OrderByDescending(x => x.LastWriteTimeUtc)
                .ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private static void EnsureMetadata(DesktopIcon icon)
        {
            // Normalize on every restore. This prevents stale metadata from
            // splitting extensions such as PNG, JPG and JPEG into different
            // visual groups after an upgrade or an Explorer refresh.
            icon.Category = DesktopItemClassifier.Classify(icon.Path, icon.IsVirtual);
            if (icon.LastWriteTimeUtc == default(DateTime))
                icon.LastWriteTimeUtc = DesktopItemClassifier.GetWriteTimeUtc(icon.Path);
        }

        private static DesktopIcon FindCurrent(
            SnapshotItem target,
            IList<DesktopIcon> currentIcons,
            Dictionary<string, DesktopIcon> currentByKey,
            HashSet<string> matchedTargetKeys)
        {
            DesktopIcon current;
            if (currentByKey.TryGetValue(target.Key, out current)
                && !matchedTargetKeys.Contains(current.Key))
                return current;
            return currentIcons
                .Where(x => !matchedTargetKeys.Contains(x.Key)
                    && string.Equals(
                        x.DisplayName,
                        target.DisplayName,
                        StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(x =>
                {
                    long dx = (long)x.X - target.X;
                    long dy = (long)x.Y - target.Y;
                    return dx * dx + dy * dy;
                })
                .FirstOrDefault();
        }

        private static List<Cell> BuildFreeCells(
            HashSet<string> occupied,
            int startColumn,
            int columns,
            int rows,
            bool allowWrap)
        {
            List<Cell> result = new List<Cell>();
            int passes = allowWrap ? 2 : 1;
            for (int pass = 0; pass < passes; pass++)
            {
                int from = pass == 0 ? Math.Min(startColumn, columns) : 0;
                int to = pass == 0 ? columns : Math.Min(startColumn, columns);
                for (int column = from; column < to; column++)
                {
                    for (int row = 0; row < rows; row++)
                    {
                        Cell cell = new Cell(column, row);
                        if (!occupied.Contains(cell.Key))
                            result.Add(cell);
                    }
                }
            }
            return result;
        }

        private static List<Cell> BuildWholeFreeColumnCells(
            HashSet<string> occupied,
            int maxTargetColumn,
            int columns,
            int rows,
            out int availableColumns,
            out int firstColumn)
        {
            List<int> candidates = new List<int>();
            int afterTargets = Math.Min(columns, Math.Max(0, maxTargetColumn + 1));

            // Prefer the open area to the right, then reuse any completely empty
            // columns inside the existing span. Isolated holes are never filled.
            for (int column = afterTargets; column < columns; column++)
                if (IsWholeColumnFree(occupied, column))
                    candidates.Add(column);
            for (int column = 0; column < afterTargets; column++)
                if (IsWholeColumnFree(occupied, column))
                    candidates.Add(column);

            availableColumns = candidates.Count;
            firstColumn = candidates.Count == 0 ? columns : candidates[0];
            List<Cell> result = new List<Cell>(candidates.Count * rows);
            foreach (int column in candidates)
                // Desktop columns are always populated vertically first.
                for (int row = 0; row < rows; row++)
                    result.Add(new Cell(column, row));
            return result;
        }

        private static bool IsWholeColumnFree(
            HashSet<string> occupied, int column)
        {
            string prefix = column + ":";
            foreach (string cellKey in occupied)
                if (cellKey.StartsWith(prefix, StringComparison.Ordinal))
                    return false;
            return true;
        }

        private static int GetOrigin(IEnumerable<int> positions, int spacing)
        {
            int[] values = positions.ToArray();
            if (values.Length == 0)
                return 0;
            int min = values.Min();
            int remainder = min % spacing;
            return remainder < 0 ? remainder + spacing : remainder;
        }

        private static Cell ToCell(
            int x, int y, int originX, int originY, int spacingX, int spacingY)
        {
            int column = Math.Max(0,
                (int)Math.Round((double)(x - originX) / spacingX));
            int row = Math.Max(0,
                (int)Math.Round((double)(y - originY) / spacingY));
            return new Cell(column, row);
        }

        private sealed class Cell
        {
            public Cell(int column, int row)
            {
                Column = column;
                Row = row;
            }
            public int Column { get; private set; }
            public int Row { get; private set; }
            public string Key { get { return Column + ":" + Row; } }
        }
    }
}
