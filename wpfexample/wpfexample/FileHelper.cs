using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace wpfexample
{
    internal static class FileHelper
    {
        private static Dictionary<string, StreamWriter> writers = new Dictionary<string, StreamWriter>();

        internal static bool FileExists(string baseDir, string fileName)
        {
            string path = Path.Combine(baseDir, fileName);
            
            return File.Exists(path);
        }

        internal static void CopyFile(string baseDir, string fileName, string destDir, string destFileName)
        {
            string path = Path.Combine(baseDir, fileName);
            string destPath = Path.Combine(destDir, destFileName);

            File.Copy(path,destPath,true);
        }
       
        internal static bool CreateFile(string baseDir, string fileName)
        {
            string path = Path.Combine(baseDir, fileName);
            FileInfo file = new FileInfo(path);
            StreamWriter writer = file.CreateText();

            writers.Add(string.Intern(fileName), writer);

            return true;
        }

        internal static void WriteLine(string fileName, string text)
        {
            Write(fileName, text);
            Write(fileName, Environment.NewLine);
            writers[fileName].Flush();
        }
        internal static void WriteLine(string fileName, object[] rawData)
        {
            Write(fileName, rawData);
            Write(fileName, Environment.NewLine);
            writers[fileName].Flush();
        }

        internal static void Write(string fileName, object[] rawData)
        {
            writers[fileName].Write(string.Join(",", rawData));
        }
        internal static void Write(string fileName, string text)
        {
            writers[fileName].Write(text);
        }

        internal static void Close(string fileName)
        {
            if (writers.ContainsKey(fileName))
            {
                writers[fileName].Flush();
                writers[fileName].Close();
                writers.Remove(fileName);
            }
        }
    }
}
