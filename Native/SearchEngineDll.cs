using System.Runtime.InteropServices;
using System.Text;

namespace Anything_UI_WPF.Native;

internal static class SearchEngineDll
{
    private const string DllName = "searchengine.dll";

    public static bool IsAvailable { get; }

    static SearchEngineDll()
    {
        try
        {
            var ptr = NativeLibrary.Load(DllName);
            NativeLibrary.Free(ptr);
            IsAvailable = true;
        }
        catch
        {
            IsAvailable = false;
        }
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int load_index_from_file(string path);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ulong search_query(string query, int search_type);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint get_result_by_index(ulong idx);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong index_size();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_c_string(nint ptr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int start_build_index(string path);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int build_index_status();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong build_index_progress();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void cancel_build_index();

    public static string? GetResultByIndex(ulong idx)
    {
        var ptr = get_result_by_index(idx);
        if (ptr == nint.Zero) return null;
        try
        {
            var len = 0;
            while (Marshal.ReadByte(ptr, len) != 0) len++;
            var bytes = new byte[len];
            Marshal.Copy(ptr, bytes, 0, len);
            return Encoding.UTF8.GetString(bytes);
        }
        finally
        {
            free_c_string(ptr);
        }
    }
}
