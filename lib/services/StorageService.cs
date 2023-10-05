using System.Threading.Tasks;
using lib.models.configuration;
using Minio;
using lib.models.exceptions;
using System.Net;
using System;

namespace lib.services
{
    public interface IStorageService : IDisposable
    {
        Task CreateBucket(string bucketName);
        Task DeleteBucket(string bucketName);
        Task<string> CreatePresignedPutUrl(string bucketName, string objectName);
        Task<string> CreatePresignedGetUrl(string bucketName, string objectName);
        Task DeleteObject(string bucketName, string objectName);

    }

    public class MinioStorageService : IStorageService
    {
        private MinioClient _client;
        private int _presignedExpirySeconds { get;} = 3600;
        public MinioStorageService(
            AppConfiguration appConfiguration
        ) 
        {
            string endpoint = appConfiguration.Storage.Port != null ? 
                $"{appConfiguration.Storage.Host}:{appConfiguration.Storage.Port}" :
                appConfiguration.Storage.Host;
            _presignedExpirySeconds = appConfiguration.Storage.PresignedExpirySeconds;

            // var httpClient = new HttpClient();

            // httpClient.BaseAddress = new System.Uri(endpoint);
            // httpClient.DefaultRequestHeaders.Add("User-Agent", "cvops-hub");
            // httpClient.DefaultRequestHeaders.Add("Host", appConfiguration.Storage.PublicHost);

            WebProxy proxy = new WebProxy(appConfiguration.Storage.Host, appConfiguration.Storage.Port ?? 80);

            _client = new MinioClient()
                            .WithEndpoint(appConfiguration.Storage.PublicHost)
                            .WithCredentials(appConfiguration.Storage.AccessKey, appConfiguration.Storage.Secret)
                            .WithProxy(proxy)
                            .WithSSL(appConfiguration.Storage.UseTls)
                            .Build();
        }

        public async Task CreateBucket(string bucketName)
        {
            bool exists = await BucketExists(bucketName);
            if (!exists)
            {
                var createBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _client.MakeBucketAsync(createBucketArgs);
            }
        }

        private async Task<bool> BucketExists(string bucketName)
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
            return await _client.BucketExistsAsync(bucketExistsArgs);
        }

        private async Task<bool> ObjectExists(string bucketName, string objectName)
        {
            var statObjectArgs = new StatObjectArgs().WithBucket(bucketName).WithObject(objectName);
            var statResult = await _client.StatObjectAsync(statObjectArgs);
            return statResult.Size > 0;
        }

        public async Task<string> CreatePresignedGetUrl(string bucketName, string objectName)
        {
            var bucketExists = await BucketExists(bucketName);
            if (bucketExists) {
                var objectExists = await ObjectExists(bucketName, objectName);
                if (objectExists) {
                    var presignedGetObjectArgs = new PresignedGetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithExpiry(_presignedExpirySeconds);
                    return await _client.PresignedGetObjectAsync(presignedGetObjectArgs);
                }
            }
            throw new StorageException($"Object {objectName} does not exist in bucket {bucketName}");
        }

        public async Task<string> CreatePresignedPutUrl(string bucketName, string objectName)
        {
            var bucketExists = await BucketExists(bucketName);
            if (!bucketExists) {
                await CreateBucket(bucketName);
            }
            var presignedPutObjectArgs = new PresignedPutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(_presignedExpirySeconds);
            return await _client.PresignedPutObjectAsync(presignedPutObjectArgs);
        }

        public async Task DeleteBucket(string bucketName)
        {
            bool bucketExists = await BucketExists(bucketName);
            if (bucketExists)
            {
                var removeBucketArgs = new RemoveBucketArgs().WithBucket(bucketName);
                await _client.RemoveBucketAsync(removeBucketArgs);
            }
        }

        public async Task DeleteObject(string bucketName, string objectName)
        {
            var bucketExists = await BucketExists(bucketName);
            if (bucketExists) {
                var objectExists = await ObjectExists(bucketName, objectName);
                if (objectExists) {
                    var removeObjectArgs = new RemoveObjectArgs().WithBucket(bucketName).WithObject(objectName);
                    await _client.RemoveObjectAsync(removeObjectArgs);
                }
            }
            
        }

        public void Dispose() {}
    }
}