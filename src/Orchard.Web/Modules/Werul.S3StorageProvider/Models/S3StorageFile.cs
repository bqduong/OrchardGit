using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.FileSystems.Media;
using Amazon.S3.Model;
using System.Collections.Specialized;
using System.IO;
using Amazon.S3;

namespace Werul.S3StorageProvider.Models
{
    internal class S3StorageFile : IStorageFile
    {
        private readonly S3Object _entry;
        private readonly string _contentType;

        public S3StorageFile(S3Object entry, string contentType)
        {
            // TODO: Complete member initialization
            this._entry = entry;
            this._contentType = contentType;
        }

        public string GetPath()
        {
            return _entry.Key;
        }

        public string GetName()
        {
            return _entry.Key.Substring(_entry.Key.LastIndexOf("/") + 1);
        }

        public long GetSize()
        {
            return _entry.Size;
        }

        public DateTime GetLastUpdated()
        {
            return DateTime.Parse(_entry.LastModified);
        }

        public string GetFileType()
        {
            return _contentType;
        }

        public System.IO.Stream OpenRead()
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream OpenWrite()
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream CreateFile()
        {
            throw new NotImplementedException();
        }
    }
}