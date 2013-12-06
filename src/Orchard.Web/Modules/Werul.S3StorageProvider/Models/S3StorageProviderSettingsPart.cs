using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Werul.S3StorageProvider.Models
{
    public class S3StorageProviderSettingsPart : ContentPart<S3StorageProviderSettingsRecord>
    {
        [Required]
        [DisplayName("AWS Access Key")]
        public virtual string AWSAccessKey
        {
            get { return Record.AWSAccessKey; }
            set { Record.AWSAccessKey = value; }
        }

        [Required]
        [DisplayName("AWS Secret Key")]
        public virtual string AWSSecretKey
        {
            get { return Record.AWSSecretKey; }
            set { Record.AWSSecretKey = value; }
        }

        [Required]
        [DisplayName("S3 Bucket Name")]
        public virtual string BucketName
        {
            get { return Record.BucketName; }
            set { Record.BucketName = value; }
        }
    }
}