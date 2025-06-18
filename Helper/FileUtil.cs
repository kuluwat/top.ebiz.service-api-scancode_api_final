using System;
using System.IO;
using top.ebiz.helper;

public class FileUtil
{
    public static FileInfo? GetFileInfo(string fullpathExternal)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fullpathExternal))
            {
                throw new ArgumentException("Path is empty or null.");
            }

            string allowedRootPath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);

            string resolvedPath = fullpathExternal;

            if (!resolvedPath.StartsWith(allowedRootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Access denied: Attempted path traversal.");
            }

            if (resolvedPath.Contains("..") || resolvedPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException("Potentially unsafe path detected.");
            }

            string fileName = Path.GetFileName(resolvedPath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(resolvedPath);

            string[] reservedNames = { "CON", "PRN", "NUL", "AUX", "COM1", "LPT1" };
            if (reservedNames.Contains(fileNameWithoutExt.ToUpper()))
            {
                throw new ArgumentException("Reserved filename detected.");
            }

            if (!File.Exists(resolvedPath))
            {
                throw new FileNotFoundException("File does not exist.", resolvedPath);
            }

            return new FileInfo(resolvedPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            LoggerFile.WarnLog($"Security error: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            LoggerFile.WarnLog($"File error: {ex.Message}");
        }
        catch (Exception ex)
        {
            LoggerFile.WarnLog($"General error: {ex.Message}");
        }

        return null;
    }
    public static DirectoryInfo? GetDirectoryInfo(string fullpathExternal)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fullpathExternal))
            {
                throw new ArgumentException("Path is empty or null.");
            }

            string allowedRootPath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);

            string resolvedPath = fullpathExternal;

            if (!resolvedPath.StartsWith(allowedRootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Access denied: Attempted path traversal.");
            }

            if (resolvedPath.Contains("..") || resolvedPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException("Potentially unsafe path detected.");
            }



            if (!Directory.Exists(resolvedPath))
            {
                Directory.CreateDirectory(resolvedPath);
            }

            return new DirectoryInfo(resolvedPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Security error: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
        }

        return null;
    }
}
