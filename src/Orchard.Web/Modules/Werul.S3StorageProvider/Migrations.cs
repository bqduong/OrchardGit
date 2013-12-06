using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Werul.S3StorageProvider
{
    public class Migrations : DataMigrationImpl {
    
        public int Create() {
            SchemaBuilder.CreateTable("S3StorageProviderSettingsRecord", table => table
                .ContentPartRecord()
                .Column<string>("AWSAccessKey")
                .Column<string>("AWSSecretKey")
                .Column<string>("BucketName")
               );

            return 1;
        }
    }
}