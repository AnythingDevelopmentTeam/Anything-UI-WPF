using System.Threading;
using System.Threading.Tasks;
using Anything_UI_WPF.Native;

namespace Anything_UI_WPF.Services;

public class IndexService
{
    public bool IsAvailable => SearchEngineDll.IsAvailable;

    public async Task<bool> StartIndexingAsync(IProgress<int>? progress = null, CancellationToken ct = default)
    {
        if (!SearchEngineDll.IsAvailable) return false;

        var indexPath = AppConfig.GetIndexPath();
        var dir = System.IO.Path.GetDirectoryName(indexPath);
        if (dir != null) System.IO.Directory.CreateDirectory(dir);

        var result = SearchEngineDll.start_build_index(indexPath);
        if (result != 0) return false;

        while (!ct.IsCancellationRequested)
        {
            var status = SearchEngineDll.build_index_status();
            if (status == 2) return true;
            if (status == 3) return false;
            if (status == 0) return false;

            var p = (int)SearchEngineDll.build_index_progress();
            progress?.Report(p);

            try { await Task.Delay(500, ct); }
            catch (TaskCanceledException) { break; }
        }

        SearchEngineDll.cancel_build_index();
        await Task.Delay(200);
        return false;
    }
}