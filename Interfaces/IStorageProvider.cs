using System.Collections.Generic;

namespace EonDB
{
    public interface IStorageProvider
    {
        void Initialize(string basePath);
        void SaveFile(string path, byte[] data);
        byte[] ReadFile(string path);
        void DeleteFile(string path);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        IEnumerable<string> ListFiles(string path);
        bool FileExists(string path);
        bool DirectoryExists(string path);
    }
}
