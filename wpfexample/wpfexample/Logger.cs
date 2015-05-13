using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace wpfexample
{
    // Quick and dirty
    internal class LoggingHelper
    {
        const string BaseDir = @"\\adsrv133\volarb\MLP\VTDev\apps\Risksystem\log";
        const string FilePrefix = "risksystem";

        private static long? _fileDate = DateTime.Now.ToFileTime();

        private static string _debugFileName;
        private static string _errorFileName;
        private static string _memoFileName;
        private static string _warningsFileName;

        //internal static readonly string DebugFileName = string.Intern(FilePrefix + "_Debug_" + FileDate + ".txt");
        //internal static readonly string WarningsFileName = string.Intern(FilePrefix + "_Warnings_" + FileDate + ".txt");
        //internal static readonly string MemoFileName = string.Intern(FilePrefix + "_Memo_" + FileDate + ".txt");
        //internal static readonly string ErrorFileName = string.Intern(FilePrefix + "_Error_" + FileDate + ".txt");

        private static bool _debugFileCreated;
        private static bool _warningsFileCreated;
        private static bool _memoFileCreated;
        private static bool _errorFileCreated;


        internal static string DebugFileName
        {
            get
            {
                _debugFileName = _debugFileName ?? string.Intern(FilePrefix + "_Debug_" + FileDate + ".txt");
                return _debugFileName;
            }
        }

        internal static string MemoFileName
        {
            get
            {
                _errorFileName = _errorFileName ?? string.Intern(FilePrefix + "_Memo_" + FileDate + ".txt");
                return _errorFileName;
            }
        }

        internal static string ErrorFileName
        {
            get
            {
                _memoFileName = _memoFileName ?? string.Intern(FilePrefix + "_Error_" + FileDate + ".txt");
                return _memoFileName;
            }
        }

        internal static string WarningsFileName
        {
            get
            {
                _warningsFileName = _warningsFileName ?? string.Intern(FilePrefix + "_Warnings_" + FileDate + ".txt");
                return _warningsFileName;
            }
        }

        private static long FileDate
        {
            get
            {
                _fileDate = _fileDate ?? DateTime.Now.ToFileTime();
                return (long)_fileDate;
            }
        }


        private static void CreateLoggingFile(ref bool wasCreated, string fileName)
        {
            if (!wasCreated)
            {
                FileHelper.CreateFile(BaseDir, fileName);
                wasCreated = true;
            }
        }

        internal static void LogDebug(string text)
        {
            CreateLoggingFile(ref _debugFileCreated, DebugFileName);

            FileHelper.WriteLine(DebugFileName, text);
        }

        internal static void LogError(string text)
        {
            CreateLoggingFile(ref _errorFileCreated, ErrorFileName);

            FileHelper.WriteLine(ErrorFileName, text);
        }

        internal static void LogMemo(string text)
        {
            CreateLoggingFile(ref _memoFileCreated, MemoFileName);

            FileHelper.WriteLine(MemoFileName, text);
        }

        internal static void LogWarn(string text)
        {
            CreateLoggingFile(ref _warningsFileCreated, WarningsFileName);

            FileHelper.WriteLine(WarningsFileName, text);

            LogMemo(text);
        }

        internal static void LogError(string text, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                LogError(string.Format(text, args));
            }
            else
            {
                LogError(text);
            }
        }

        internal static void LogMemo(string text, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                LogMemo(string.Format(text, args));
            }
            else
            {
                LogMemo(text);
            }
        }

        internal static void LogWarn(string text, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                LogWarn(string.Format(text, args));
            }
            else
            {
                LogWarn(text);
            }
        }

        internal static void CloseLogs()
        {
            _fileDate = null;

            CloseLogFile(DebugFileName, ref _debugFileCreated);
            CloseLogFile(ErrorFileName, ref _errorFileCreated);
            CloseLogFile(MemoFileName, ref _memoFileCreated);
            CloseLogFile(WarningsFileName, ref _warningsFileCreated);
        }

        private static void CloseLogFile(string fileName, ref bool fileCreated)
        {
            fileCreated = false;
            FileHelper.Close(fileName);
        }

        internal static string GetFullFilePath(string fileName)
        {
            return Path.Combine(BaseDir, fileName);
        }
    }
}
