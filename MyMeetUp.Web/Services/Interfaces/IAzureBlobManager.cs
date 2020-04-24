using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyMeetUp.Web.Services.Interfaces
{
    public interface IAzureBlobManager
    {
        Task<string> UploadFile(IFormFile myFile, string folderHierachy);
        Task<List<IListBlobItem>> BlobList();
        Task<List<IListBlobItem>> ListBlobsHierarchicalListingAsync(string prefix);
        Task<bool> DeleteBlob(string absoluteUri);
    }
}
