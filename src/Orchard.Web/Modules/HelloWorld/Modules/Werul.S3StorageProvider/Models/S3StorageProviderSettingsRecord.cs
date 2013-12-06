using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Werul.S3StorageProvider.Models
{
    public class S3StorageProviderSettingsRecord : ContentPartRecord
    {
        [Required]
        [DisplayName("AWS Access Key")]
        public virtual string AWSAccessKey { get; set; }

        [Required]
        [DisplayName("AWS Secret Key")]
        public virtual string AWSSecretKey { get; set; }

        [Required]
        [DisplayName("S3 Bucket Name")]
        public virtual string BucketName { get; set; }
    }
}