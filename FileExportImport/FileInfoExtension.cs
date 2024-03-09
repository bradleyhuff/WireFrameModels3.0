
namespace FileExportImport
{
    public static class FileInfoExtension
    {
        public static string DisplayFileSize(this FileInfo info)
        {
            if (info.Length < 1e3) { return $"{info.Length} bytes"; }
            if (info.Length < 1e4) { return $"{((double)info.Length / 1024).ToString("0.00")} Kbytes"; }
            if (info.Length < 1e5) { return $"{((double)info.Length / 1024).ToString("0.0")} Kbytes"; }
            if (info.Length < 1e6) { return $"{((double)info.Length / 1024).ToString("0")} Kbytes"; }
            if (info.Length < 1e7) { return $"{((double)info.Length / 1048576).ToString("0.00")} Mbytes"; }
            if (info.Length < 1e8) { return $"{((double)info.Length / 1048576).ToString("0.0")} Mbytes"; }
            if (info.Length < 1e9) { return $"{((double)info.Length / 1048576).ToString("0")} Mbytes"; }
            if (info.Length < 1e10) { return $"{((double)info.Length / 1073741824).ToString("0.00")} Gbytes"; }
            if (info.Length < 1e11) { return $"{((double)info.Length / 1073741824).ToString("0.0")} Gbytes"; }
            return $"{((double)info.Length / 1073741824).ToString("#,##0")} Gbytes";
        }
    }
}
