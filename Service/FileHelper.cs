
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace top.ebiz.service.Service
{
    public class FileHelper
    {
        public static string SanitizeLog(string input)
        {
            return input?.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }
        public static string SanitizePath(string input)
        {
            // เอาอักขระต้องห้ามออก เช่น ../ หรือ \ หรือ :
            return Path.GetFileName(input).Replace("..", "").Replace("/", "").Replace("\\", "");
        }
    }
}

