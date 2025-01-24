using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace EonDB
{
    public class AmazonS3StorageProvider : IStorageProvider
    {
        private AmazonS3Client _client;
        private string _bucketName;

        public void Initialize(string connectionString)
        {
            var parts = connectionString.Split(';');
            _bucketName = parts[0];
            var accessKey = parts[1];
            var secretKey = parts[2];

            _client = new AmazonS3Client(accessKey, secretKey);
        }

        public void SaveFile(string path, byte[] data)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = path,
                InputStream = new MemoryStream(data)
            };
            _client.PutObjectAsync(request).Wait();
        }

        public byte[] ReadFile(string path)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = path
            };

            using var response = _client.GetObjectAsync(request).Result;
            using var memoryStream = new MemoryStream();
            response.ResponseStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public void DeleteFile(string path)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = path
            };
            _client.DeleteObjectAsync(request).Wait();
        }

        public void CreateDirectory(string path) { }

        public void DeleteDirectory(string path)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = path
            };

            var response = _client.ListObjectsV2Async(request).Result;
            foreach (var obj in response.S3Objects)
            {
                DeleteFile(obj.Key);
            }
        }

        public IEnumerable<string> ListFiles(string path)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = path
            };

            var response = _client.ListObjectsV2Async(request).Result;
            foreach (var obj in response.S3Objects)
                yield return obj.Key;
        }

        public bool FileExists(string path)
        {
            try
            {
                ReadFile(path);
                return true;
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
}
