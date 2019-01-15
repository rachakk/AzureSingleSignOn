using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AzureLoginAndBlob.Controllers
{
    [RoutePrefix("api/upload")]
    public class UploadController : ApiController
    {
        [HttpPost]
        [Route("file")]
        public async Task<HttpResponseMessage> UploadFileInAzure()
        {
            if (!Request.Content.IsMimeMultipartContent())
                this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);

            var root = HttpContext.Current.Server.MapPath("~/temp/uploads");
            var provider = new MultipartFormDataStreamProvider(root);
             var result = await Request.Content.ReadAsMultipartAsync(provider);

            if(!result.FileData.Any())
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var ticks = DateTime.Now.Ticks.ToString();

            var originalFileName = JsonConvert.DeserializeObject(result.FileData.First().Headers.ContentDisposition.FileName).ToString();

            var uploadFile = new FileInfo(result.FileData.First().LocalFileName);
            var uploadFileName = $"{ originalFileName.Split('.').First() }_{ ticks }.{ originalFileName.Split('.').Last() }";
            var storageFileName = $"files\\{uploadFileName}";


             var bloburl = SaveProjectFile(uploadFile, storageFileName);
            return this.Request.CreateResponse(HttpStatusCode.Accepted, bloburl);
        }

        public  string SaveProjectFile(FileInfo file, string FileName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("asset");

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(FileName);

            using (var stream = file.OpenRead())
            {
                blockBlob.UploadFromStream(stream);
                //uploadPath.Delete();
                var uri = blockBlob.Uri.AbsoluteUri;
                return uri;
            }            
        }
    }
}
