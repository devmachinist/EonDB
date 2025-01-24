using Azure.Storage.Blobs;
using EonDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EonDB;

public class AzureBlobStorageProvider : IStorageProvider
{
    private BlobServiceClient _blobServiceClient;
    private BlobContainerClient _containerClient;

    public void Initialize(string connectionString)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient("eondb");
        _containerClient.CreateIfNotExists();
    }

    public void SaveFile(string path, byte[] data)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        using var stream = new MemoryStream(data);
        blobClient.Upload(stream, true);
    }

    public byte[] ReadFile(string path)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        using var ms = new MemoryStream();
        blobClient.DownloadTo(ms);
        return ms.ToArray();
    }

    public void DeleteFile(string path)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        blobClient.DeleteIfExists();
    }

    public void CreateDirectory(string path)
    {
        // Azure Blob Storage doesn't support directories, but we can simulate them by prefixing blobs
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
        var results = _containerClient.GetBlobs(prefix: path);
        foreach (var blobItem in results)
        {
            yield return blobItem.Name;
        }
    }

    public bool FileExists(string path)
    {
        var blobClient = _containerClient.GetBlobClient(path);
        return blobClient.Exists();
    }

    public bool DirectoryExists(string path)
    {
        return ListFiles(path).GetEnumerator().MoveNext();
    }
}
