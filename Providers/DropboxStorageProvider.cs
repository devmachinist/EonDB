using Dropbox.Api;
using Dropbox.Api.Files;
using EonDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class DropboxStorageProvider : IStorageProvider
{
    private DropboxClient _client;

    public void Initialize(string accessToken)
    {
        _client = new DropboxClient(accessToken);
    }

    public void SaveFile(string path, byte[] data)
    {
        using var stream = new MemoryStream(data);
        var task = _client.Files.UploadAsync( new UploadArg(path), stream);
        task.Wait();
    }

    public byte[] ReadFile(string path)
    {
        var task = _client.Files.DownloadAsync(path);
        task.Wait();
        using var stream = task.Result.GetContentAsStreamAsync().Result;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public void DeleteFile(string path)
    {
        var task = _client.Files.DeleteV2Async(path);
        task.Wait();
    }

    public void CreateDirectory(string path)
    {
        var task = _client.Files.CreateFolderV2Async(path);
        task.Wait();
    }

    public void DeleteDirectory(string path)
    {
        DeleteFile(path); // Dropbox treats folders and files the same for deletion
    }

    public IEnumerable<string> ListFiles(string path)
    {
        var task = _client.Files.ListFolderAsync(path);
        task.Wait();
        foreach (var entry in task.Result.Entries)
        {
            yield return entry.Name;
        }
    }

    public bool FileExists(string path)
    {
        try
        {
            var task = _client.Files.GetMetadataAsync(path);
            task.Wait();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DirectoryExists(string path)
    {
        return FileExists(path);
    }
}
