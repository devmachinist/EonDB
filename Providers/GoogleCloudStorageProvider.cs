using EonDB;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;

namespace EonDB;

public class GoogleCloudStorageProvider : IStorageProvider
{
    private StorageClient _client;
    private string _bucketName;

    public void Initialize(string connectionString)
    {
        // connectionString should contain the bucket name
        _bucketName = connectionString;
        _client = StorageClient.Create();
    }

    public void SaveFile(string path, byte[] data)
    {
        using var stream = new MemoryStream(data);
        _client.UploadObject(_bucketName, path, null, stream);
    }

    public byte[] ReadFile(string path)
    {
        using var ms = new MemoryStream();
        _client.DownloadObject(_bucketName, path, ms);
        return ms.ToArray();
    }

    public void DeleteFile(string path)
    {
        _client.DeleteObject(_bucketName, path);
    }

    public void CreateDirectory(string path)
    {
        // Google Cloud Storage doesn't support directories
    }

    public void DeleteDirectory(string path)
    {
        foreach (var file in ListFiles(path))
        {
            DeleteFile(file);
        }
    }

    public IEnumerable<string> ListFiles(string path)
    {
        foreach (var obj in _client.ListObjects(_bucketName, path))
        {
            yield return obj.Name;
        }
    }

    public bool FileExists(string path)
    {
        try
        {
            var obj = _client.GetObject(_bucketName, path);
            return obj != null;
        }
        catch
        {
            return false;
        }
    }

    public bool DirectoryExists(string path)
    {
        return ListFiles(path).GetEnumerator().MoveNext();
    }
}
