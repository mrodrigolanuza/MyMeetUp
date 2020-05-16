using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MyMeetUp.Web.Configuration;
using MyMeetUp.Web.Services.Interfaces;

namespace MyMeetUp.Web.Services
{
    public class AzureBlobManager : IAzureBlobManager
    {
        CloudBlobContainer _blobContainer;

        public AzureBlobManager(IOptions<BlobStorageSettings> blobStorageSettings) {
            try {
                var storageAccount = CloudStorageAccount.Parse(blobStorageSettings.Value.ConnectionString);          //Storage Account
                var cloudBlobClient = storageAccount.CreateCloudBlobClient();                       //Cliente blob sobre la storageaccount
                _blobContainer = cloudBlobClient.GetContainerReference(blobStorageSettings.Value.ContainerName);     //Contenedor blob
            } catch (Exception ex) {
                throw ex;
            }
        }

        public async Task<List<IListBlobItem>> BlobList() {
            var totalResults = new List<IListBlobItem>();
            BlobContinuationToken continuationToken = null;
            do {
                var results = await _blobContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = results.ContinuationToken;
                totalResults.AddRange(results.Results);
            } while (continuationToken!=null);
            return totalResults;
        }

        public async Task<bool> DeleteBlob(string absoluteUri) {
            try {
                var uriObj = new Uri(absoluteUri);
                var blockBlob = _blobContainer.GetBlockBlobReference(Path.GetFileName(uriObj.LocalPath));
                await blockBlob.DeleteAsync();
                return true;
            } catch (Exception ex) {
                throw ex;
            }
        }

        public async Task<string> UploadFile(IFormFile myFile, string folderHierachy) {
            var absoluteUri = string.Empty;
            if (myFile == null || myFile.Length == 0)
                return null;
            try {
                var blockBlob = _blobContainer.GetBlockBlobReference(folderHierachy + Path.GetFileName(myFile.FileName));
                blockBlob.Properties.ContentType = myFile.ContentType;  //Indicar tipo de contenido que albergará el blob (imágen, fichero..)
                await blockBlob.UploadFromStreamAsync(myFile.OpenReadStream());
                absoluteUri = blockBlob.Uri.AbsoluteUri;
            } catch (Exception ex) {
                throw ex;
            }
            return absoluteUri;
        }

        public async Task<List<IListBlobItem>> ListBlobsHierarchicalListingAsync(string prefix) {
            CloudBlobDirectory dir;
            BlobContinuationToken continuationToken;
            var totalResults = new List<IListBlobItem>();

            try {
                do {
                    BlobResultSegment resultSegment = await _blobContainer.ListBlobsSegmentedAsync(prefix, false, BlobListingDetails.Metadata, null, null, null, null);
                    foreach (var blobItem in resultSegment.Results) {
                        // A hierarchical listing may return both virtual directories and blobs.
                        if (blobItem is CloudBlobDirectory) {
                            dir = (CloudBlobDirectory)blobItem;
                            // Call recursively with the prefix to traverse the virtual directory.
                            await ListBlobsHierarchicalListingAsync(dir.Prefix);
                        }
                        else {
                            totalResults.Add(blobItem);
                        }
                    }
                    // Get the continuation token and loop until it is null.
                    continuationToken = resultSegment.ContinuationToken;
                } while (continuationToken != null);
            } catch (Exception ex) {
                throw ex;
            }
            return totalResults;
        }
    }
}
