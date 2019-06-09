using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Annium.Logging.Abstractions;

namespace Backuper.Storage.S3
{
    public class Storage : Abstract.Storage
    {
        private readonly Configuration cfg;

        private string dir;

        public Storage(
            string name,
            Configuration cfg,
            ILogger logger
        ) : base("S3", name, logger)
        {
            this.cfg = cfg;
        }

        protected override async Task DoSetupAsync()
        {
            if (Path.GetFullPath(cfg.Path) != cfg.Path)
                throw new InvalidOperationException($"Path {cfg.Path} is not absolute");

            dir = cfg.Path;

            using(var s3 = GetClient())
            {
                var buckets = (await s3.ListBucketsAsync()).Buckets.Select(b => b.BucketName).ToArray();
                if (buckets.Contains(cfg.Bucket))
                    return;

                await s3.PutBucketAsync(new PutBucketRequest { BucketName = cfg.Bucket });
            }
        }

        protected override async Task<string[]> DoListAsync(string folder)
        {
            var req = new ListObjectsRequest
            {
                BucketName = cfg.Bucket,
                MaxKeys = 100
            };

            var prefix = getPrefix(folder);

            using(var s3 = GetClient())
            {
                var objects = (await s3.ListObjectsAsync(req)).S3Objects;

                return objects
                    .Where(o => Path.GetDirectoryName(o.Key) == prefix)
                    .Select(o => readKey(folder, o.Key))
                    .ToArray();
            }
        }

        protected override async Task DoUploadAsync(string source, string folder, string name)
        {
            var key = getKey(folder, name);
            var fs = File.Open(source, FileMode.Open);
            var req = new PutObjectRequest
            {
                BucketName = cfg.Bucket,
                Key = key,
                InputStream = fs,
            };

            using(var s3 = GetClient())
            {
                await s3.PutObjectAsync(req);
            }
        }

        protected override async Task DoDownloadAsync(string folder, string name, string target)
        {
            var key = getKey(folder, name);
            var req = new GetObjectRequest
            {
                BucketName = cfg.Bucket,
                Key = key,
            };

            using(var s3 = GetClient())
            {
                using(var resStream = (await s3.GetObjectAsync(req)).ResponseStream)
                using(var tgtStream = File.Open(target, FileMode.CreateNew))
                {
                    await resStream.CopyToAsync(tgtStream);
                }
            }
        }

        protected override async Task DoDeleteAsync(string folder, string name)
        {
            var key = getKey(folder, name);
            var req = new DeleteObjectRequest
            {
                BucketName = cfg.Bucket,
                Key = key
            };

            using(var s3 = GetClient())
            {
                await s3.DeleteObjectAsync(req);
            }
        }

        private IAmazonS3 GetClient()
        {
            if (string.IsNullOrWhiteSpace(cfg.AccessKey))
                throw new ArgumentException("Access key is required");

            if (string.IsNullOrWhiteSpace(cfg.AccessSecret))
                throw new ArgumentException("Access secret is required");

            if (string.IsNullOrWhiteSpace(cfg.Bucket))
                throw new ArgumentException("Bucket name is required");

            var s3cfg = new AmazonS3Config();
            s3cfg.RegionEndpoint = RegionEndpoint.GetBySystemName(cfg.Region);
            if (!string.IsNullOrWhiteSpace(cfg.Server))
                s3cfg.ServiceURL = cfg.Server;

            return new AmazonS3Client(cfg.AccessKey, cfg.AccessSecret, s3cfg);
        }

        private string getPrefix(string folder) =>
            Path.Combine(cfg.Path.Substring(1), folder);

        private string getKey(string folder, string name) =>
            Path.Combine(cfg.Path.Substring(1), folder, name);

        private string readKey(string folder, string key) =>
            Path.GetRelativePath(Path.Combine(cfg.Path, folder), $"/{key}");
    }
}