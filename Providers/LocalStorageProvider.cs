using System;
using System.Collections.Generic;
using System.IO;

namespace EonDB
{
    public class LocalStorageProvider : IStorageProvider
    {
        private string _basePath;

        public void Initialize(string basePath)
        {
            _basePath = basePath ?? AppDomain.CurrentDomain.BaseDirectory + "/EonDB";

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public void SaveFile(string path, byte[] data)
        {
            var fullPath = Path.Combine(_basePath, path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, data);
        }

        public byte[] ReadFile(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {fullPath}");
            return File.ReadAllBytes(fullPath);
        }

        public void DeleteFile(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public void CreateDirectory(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
        }

        public void DeleteDirectory(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath, true);
        }

        public IEnumerable<string> ListFiles(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            if (!Directory.Exists(fullPath))
                return new List<string>();

            return Directory.GetFiles(fullPath);
        }

        public bool FileExists(string path)
        {
            return File.Exists(Path.Combine(_basePath, path));
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(Path.Combine(_basePath, path));
        }
    }
}
