using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Validation;
using Orchard.FileSystems.Media;
using Amazon.S3.Model;
using Werul.S3StorageProvider.Models;
using Orchard.Environment.Extensions;
using Amazon.S3;
using Orchard;
using Orchard.ContentManagement;

namespace Werul.S3StorageProvider.Providers {
    [OrchardSuppressDependency("Orchard.FileSystems.Media.FileSystemStorageProvider")]
    public class S3StorageProvider : IStorageProvider {

        private readonly AmazonS3Config _S3Config;
        private readonly IOrchardServices Services;        

        public S3StorageProvider(IOrchardServices services)
        {
            Services = services;
            _S3Config = new AmazonS3Config()
            {
                ServiceURL = "s3.amazonaws.com",
                CommunicationProtocol = Amazon.S3.Model.Protocol.HTTP,
            };

            T = NullLocalizer.Instance;
        }

        public string PublicPath
        {
            get
            {
                return string.Format("https://s3.amazonaws.com/{0}/", BucketName);
            }
        }
        public string AWSAccessKey
        {
            get
            {
                return Services.WorkContext.CurrentSite.As<S3StorageProviderSettingsPart>().Record.AWSAccessKey;
            }
        }
        public string AWSSecretKey
        {
            get
            {
                return Services.WorkContext.CurrentSite.As<S3StorageProviderSettingsPart>().Record.AWSSecretKey;
            }
        }
        public string BucketName
        {
            get
            {
                return Services.WorkContext.CurrentSite.As<S3StorageProviderSettingsPart>().Record.BucketName;
            }
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Retrieves the public URL for a given file within the storage provider.
        /// </summary>
        /// <param name="path">The relative path within the storage provider.</param>
        /// <returns>The public URL.</returns>
        public string GetPublicUrl(string path)
        {
            return string.IsNullOrEmpty(path) ? PublicPath : Path.Combine(PublicPath, path).Replace(Path.DirectorySeparatorChar, '/');
        }

        /// <summary>
        /// Retrieves a file within the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the file within the storage provider.</param>
        /// <returns>The file.</returns>
        /// <exception cref="ArgumentException">If the file is not found.</exception>
        public IStorageFile GetFile(string path)
        {
            //not sure if we should use a temp directory here or what
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lists the files within a storage provider's path.
        /// </summary>
        /// <param name="path">The relative path to the folder which files to list.</param>
        /// <returns>The list of files in the folder.</returns>
        public IEnumerable<IStorageFile> ListFiles(string path)
        {
            var files = new List<S3StorageFile>();
            using (var client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, _S3Config))
            {
                var request = new ListObjectsRequest();
                request.BucketName = BucketName;
                request.Prefix = path;
                using (ListObjectsResponse response = client.ListObjects(request))
                {                    
                    foreach (var entry in response.S3Objects.Where(e => e.Key.Last() != '/' && e.Key.Count(c => c == '/') == path.Count(c => c == '/')))
                    {
                        var mimeType = GetMimeType(entry.Key.Substring(entry.Key.LastIndexOf(".")));
                        files.Add(new S3StorageFile(entry, mimeType));
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// Lists the folders within a storage provider's path.
        /// </summary>
        /// <param name="path">The relative path to the folder which folders to list.</param>
        /// <returns>The list of folders in the folder.</returns>
        public IEnumerable<IStorageFolder> ListFolders(string path)
        {
            var folders = new List<S3StorageFolder>();
            using (var client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, _S3Config))
            {
                path = path ?? "";
                var request = new ListObjectsRequest();
                request.BucketName = BucketName;
                request.Prefix = path;
                using (ListObjectsResponse response = client.ListObjects(request))
                {
                    foreach (var entry in response.S3Objects.Where(e => e.Key.Last() == '/' && e.Key.Count(c => c == '/') == path.Count(c => c == '/') + 1))
                    {
                        folders.Add(new S3StorageFolder(entry));
                    }
                }
            }
            return folders;
        }

        /// <summary>
        /// Tries to create a folder in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the folder to be created.</param>
        /// <returns>True if success; False otherwise.</returns>
        public bool TryCreateFolder(string path)
        {
            try
            {
                using (var client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, _S3Config))
                {
                    var key = string.Format(@"{0}/", path);
                    var request = new PutObjectRequest().WithBucketName(BucketName).WithKey(key);
                    request.InputStream = new MemoryStream();
                    client.PutObject(request);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a folder in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the folder to be created.</param>
        /// <exception cref="ArgumentException">If the folder already exists.</exception>
        public void CreateFolder(string path)
        {
            try
            {
                using (var client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, _S3Config))
                {
                    var key = string.Format(@"{0}/", path);
                    var request = new PutObjectRequest().WithBucketName(BucketName).WithKey(key);
                    request.InputStream = new MemoryStream();
                    client.PutObject(request);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(T("Directory {0} already exists", path).ToString(), ex);
            }
        }

        public void DeleteFolder(string path)
        {
            //TODO: recursive on all keys with prefix
            using (var client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, _S3Config))
            {
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(BucketName)
                    .WithKey(path);
                using (DeleteObjectResponse response = client.DeleteObject(request))
                {

                }
            }
        }

        public void RenameFolder(string oldPath, string newPath)
        {
            //TODO: recursive on all keys with prefix
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            using (var client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, _S3Config))
            {
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.WithBucketName(BucketName)
                    .WithKey(path);
                using (DeleteObjectResponse response = client.DeleteObject(request))
                {

                }
            }
        }

        public void RenameFile(string oldPath, string newPath)
        {
            throw new NotImplementedException();
        }

        public IStorageFile CreateFile(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to save a stream in the storage provider.
        /// </summary>
        /// <param name="path">The relative path to the file to be created.</param>
        /// <param name="inputStream">The stream to be saved.</param>
        /// <returns>True if success; False otherwise.</returns>
        public bool TrySaveStream(string path, Stream inputStream)
        {
            try
            {
                SaveStream(path, inputStream);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void SaveStream(string path, Stream inputStream)
        {
            using (var client = Amazon.AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey, _S3Config))
            {
                PutObjectRequest request = new PutObjectRequest();
                request.WithBucketName(BucketName).WithKey(path).WithCannedACL(S3CannedACL.PublicRead).WithInputStream(inputStream);
                using (var response = client.PutObject(request))
                {

                }                
            }
        }

        public string Combine(string path1, string path2)
        {
            return path1 + path2;
        }

        private string GetMimeType(string ext)
        {
            string mimeType = "application/unknown";
            ext = ext.ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
    }
}
