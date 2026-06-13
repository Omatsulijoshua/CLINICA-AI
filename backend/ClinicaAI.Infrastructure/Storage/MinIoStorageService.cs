using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace ClinicaAI.Infrastructure.Storage
{
    public class MinIoStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly AmazonS3Client? _s3Client;
        private readonly string _bucketName;
        private readonly string _localFallbackDir;

        public MinIoStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _bucketName = _configuration["MinIO:BucketName"] ?? "clinica-files";
            _localFallbackDir = Path.Combine(AppContext.BaseDirectory, "App_Data", "Uploads");

            if (!Directory.Exists(_localFallbackDir))
            {
                Directory.CreateDirectory(_localFallbackDir);
            }

            try
            {
                var endpoint = _configuration["MinIO:Endpoint"] ?? "localhost:9000";
                var accessKey = _configuration["MinIO:AccessKey"] ?? "minioadmin";
                var secretKey = _configuration["MinIO:SecretKey"] ?? "minioadminpassword";

                if (!string.IsNullOrEmpty(endpoint))
                {
                    var s3Config = new AmazonS3Config
                    {
                        ServiceURL = endpoint.StartsWith("http") ? endpoint : $"http://{endpoint}",
                        ForcePathStyle = true
                    };
                    _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MinIO Storage Warning] Failed to initialize MinIO client. Falling back to local disk storage. Detail: {ex.Message}");
                _s3Client = null;
            }
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

            if (_s3Client != null)
            {
                try
                {
                    // Ensure bucket exists
                    try
                    {
                        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _bucketName });
                    }
                    catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict || ex.ErrorCode == "BucketAlreadyOwnedByYou")
                    {
                        // Bucket already exists, ignore
                    }

                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = uniqueFileName,
                        InputStream = fileStream,
                        ContentType = contentType
                    };

                    await _s3Client.PutObjectAsync(putRequest);
                    return $"/api/files/download/{uniqueFileName}"; // Custom backend proxy path for file retrieval
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MinIO Upload Error] Failed uploading to MinIO: {ex.Message}. Storing on local disk instead.");
                }
            }

            // Local Fallback Storage
            var filePath = Path.Combine(_localFallbackDir, uniqueFileName);
            fileStream.Position = 0; // Reset stream
            using (var fileDestStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileDestStream);
            }

            return $"/api/files/download/{uniqueFileName}";
        }

        public async Task<Stream> DownloadFileAsync(string uniqueFileName)
        {
            if (_s3Client != null)
            {
                try
                {
                    var getRequest = new GetObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = uniqueFileName
                    };

                    var getResponse = await _s3Client.GetObjectAsync(getRequest);
                    return getResponse.ResponseStream;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MinIO Download Error] Failed retrieving from MinIO: {ex.Message}. Reading from local disk.");
                }
            }

            // Local Fallback
            var filePath = Path.Combine(_localFallbackDir, uniqueFileName);
            if (File.Exists(filePath))
            {
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }

            throw new FileNotFoundException("File could not be found in MinIO or local storage fallback.", uniqueFileName);
        }
    }
}
