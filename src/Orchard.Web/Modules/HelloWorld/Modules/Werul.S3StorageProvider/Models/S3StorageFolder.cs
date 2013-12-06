using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.FileSystems.Media;
using Amazon.S3.Model;

namespace Werul.S3StorageProvider.Models
{
    internal class S3StorageFolder : IStorageFolder
    {
        private readonly S3Object _entry;

        public S3StorageFolder(S3Object entry)
        {
            // TODO: Complete member initialization
            this._entry = entry;
        }

        public string GetPath()
        {
            return _entry.Key;
        }

        public string GetName()
        {
            var tempKey = _entry.Key.Substring(0, _entry.Key.Length - 1); //remove trailing slash
            if (tempKey.Contains("/"))
                tempKey = tempKey.Substring(tempKey.LastIndexOf('/') + 1);
            return tempKey;
        }

        public long GetSize()
        {
            return _entry.Size;
        }

        public DateTime GetLastUpdated()
        {
            return DateTime.Parse(_entry.LastModified);
        }

        public IStorageFolder GetParent()
        {
            throw new NotImplementedException();
        }
    }
}