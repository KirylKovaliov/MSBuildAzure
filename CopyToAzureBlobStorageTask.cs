using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace RhysG.MSBuild.Azure
{
    public class CopyToAzureBlobStorageTask : ITask
    {
        private IBuildEngine _buildEngine;
        private ITaskHost _taskHost;

        public IBuildEngine BuildEngine
        {
            get
            {
                return _buildEngine;
            }
            set
            {
                _buildEngine = value;
            }
        }

        [Required]
        public string ContainerName
        {
            get;
            set;
        }

        [Required]
        public string ConnectionString
        {
            get;
            set;
        }

        [Required]
        public string ContentType
        {
            get;
            set;
        }

        public string ContentEncoding
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Files
        {
            get;
            set;
        }

        public bool Execute()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(ConnectionString);
            CloudBlobClient blobClient = account.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Container });

            foreach (ITaskItem fileItem in Files)
            {
                FileInfo file = new FileInfo(fileItem.ItemSpec);

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.Name);
                using (var fileStream = File.OpenRead(file.FullName))
                {
                    blockBlob.UploadFromStream(fileStream);

                    BuildEngine.LogMessageEvent(new BuildMessageEventArgs(String.Format("Uploading: {0}", file.Name), String.Empty, "CopyToAzureBlobStorageTask", MessageImportance.Normal));
                }
            }

            return true;
        }

        public ITaskHost HostObject
        {
            get
            {
                return _taskHost;
            }
            set
            {
                _taskHost = value;
            }
        }
    }
}
