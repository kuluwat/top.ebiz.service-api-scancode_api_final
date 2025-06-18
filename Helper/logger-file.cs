using System;
using System.IO;
using System.Text;
namespace top.ebiz.helper;
public class LoggerFile
{ 
    /// <summary>
    /// บันทึกข้อความ Warning ลง log.txt พร้อม timestamp และรายละเอียด exception (ถ้ามี)
    /// </summary>
    /// <param name="message">ข้อความ log</param>
    /// <param name="ex">exception (optional)</param>
    public static void WarnLog(string message, Exception? ex = null)
    {
        try
        {
            var ddmmYYY = DateTime.Now.ToString("yyMMdd");
            var errorPath = Path.Combine(Environment.CurrentDirectory, "log");
            string logFilePath = Path.Combine(errorPath, $"{ddmmYYY}-WarnLog.txt");
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WARN] {message}");

            if (ex != null)
            {
                logMessage.AppendLine($"Exception: {ex.Message}");
                logMessage.AppendLine($"StackTrace: {ex.StackTrace}");
            }

            File.AppendAllText(logFilePath, logMessage.ToString());
        }
        catch
        {
            // ห้าม throw error ซ้ำจาก log
            // ถ้าอยาก log ลง EventLog หรือ fallback ก็ทำที่นี่ได้
        }
    }
    public static void write(Exception logMessage)
    {
        var ddmmYYY = DateTime.Now.ToString("yyMMdd");
        var errorPath = Path.Combine(Environment.CurrentDirectory, "log");
        string logFilePath = Path.Combine(errorPath, $"{ddmmYYY}-error-log.txt");
        // if (!File.Exists(logFilePath))
        // {
        //     File.Create(logFilePath).Close();
        // }

        // Log a message
        string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        message += Environment.NewLine;
        message += "-----------------------------------------------------------";
        message += Environment.NewLine;
        message += string.Format("Message: {0}", logMessage.Message);
        message += Environment.NewLine;
        message += string.Format("StackTrace: {0}", logMessage.StackTrace);
        message += Environment.NewLine;
        message += string.Format("Source: {0}", logMessage.Source);
        message += Environment.NewLine;
        message += string.Format("TargetSite: {0}", logMessage.TargetSite?.ToString() ?? "");
        message += Environment.NewLine;
        message += "-----------------------------------------------------------";
        message += Environment.NewLine;
        Console.WriteLine(message);
        // File.AppendAllText(logFilePath, $"{message}");
    }
}
