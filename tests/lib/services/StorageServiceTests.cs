namespace tests.lib.services
{
    // These are integration tests for minioclient
    // requires a running minio server
    // TODO: comment out for now
    public class StorageServiceTests
    {
        // [Fact]
        // public async Task CreatePresignedPutUrl_GeneratesUrl_WithPublicHost()
        // {
        //     var appConfiguration = new ConfigurationManager().Configure();
        //     var storageService = new MinioStorageService(appConfiguration);

        //     var bucketName = Guid.NewGuid().ToString();

        //     string objectName = "test.txt";

        //     string url = await storageService.CreatePresignedPutUrl(bucketName, objectName);

        //     Uri uri = new Uri(url);

        //     Assert.Equal(appConfiguration.Storage.PublicHost, uri.Host);
        // }
    }
}