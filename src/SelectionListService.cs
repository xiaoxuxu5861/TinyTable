using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TinyTable
{
    public sealed class SelectionListService
    {
        private readonly SnapshotStore _store = new SnapshotStore();

        public int UpdatePaths(IEnumerable<string> paths, bool add)
        {
            List<string> keys = paths
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => DesktopIconService.BuildKey(null, x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return UpdateKeys(keys, add);
        }

        public int UpdateKeys(IEnumerable<string> keys, bool add)
        {
            using (Mutex mutex = new Mutex(false, @"Local\TinyTable.DatabaseWrite"))
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(8)))
                    throw new InvalidOperationException("TinyTable 列表正被其他操作占用，请重试。");
                try
                {
                    SnapshotDatabase database = _store.Load();
                    HashSet<string> current = new HashSet<string>(
                        database.ProtectedKeys ?? new List<string>(),
                        StringComparer.OrdinalIgnoreCase);
                    int changed = 0;
                    foreach (string key in keys)
                    {
                        if (add)
                        {
                            if (current.Add(key)) changed++;
                        }
                        else
                        {
                            if (current.Remove(key)) changed++;
                        }
                    }
                    database.ProtectedKeys = current.OrderBy(x => x).ToList();
                    if (changed > 0)
                        database.ActiveSelectionPresetId = null;
                    _store.Save(database);
                    return changed;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
