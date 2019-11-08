using CatalogAPI.Helpers;
using CatalogAPI.Infrastructure;
using CatalogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CatalogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private CatalogContext db;
        IConfiguration config;
        public CatalogController(CatalogContext db, IConfiguration configuration)
        {
            this.db = db;
            this.config = configuration;
        }

        [AllowAnonymous]
        [HttpGet("", Name ="GetProducts")]
        public async  Task<ActionResult<List<CatalogItem>>> GetProducts()
        {
            var result = await this.db.Catalog.FindAsync<CatalogItem>(FilterDefinition<CatalogItem>.Empty);
            return result.ToList();
        }

        [Authorize(Roles = "admin")]
        [HttpPost("", Name = "AddProduct")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult<CatalogItem> AddProduct(CatalogItem item)
        {
            if (ModelState.IsValid)
            {
                this.db.Catalog.InsertOne(item);
                return Created("", item);
            }
            else
            {
                return BadRequest();
            }

        }

        [Authorize(Roles = "admin")]

        [HttpPost("product")]
        public ActionResult<CatalogItem> Addproduct()
        {
            var imageName = SaveImageToCloudAsync(Request.Form.Files[0]).GetAwaiter().GetResult();
            var catalogItem = new CatalogItem()
            {
                Name = Request.Form["name"],
                Price = Double.Parse(Request.Form["price"]),
                Quantity = Convert.ToInt32(Request.Form["quantity"]),
                RecordLevel = Convert.ToInt32(Request.Form["recordLevel"]),
                ManufacturingDate = DateTime.Parse(Request.Form["manufacturingDate"]),
                Vendors = new List<Vendor>(),
                ImageUrl = imageName
            };

            db.Catalog.InsertOne(catalogItem);
            backUpToTableAsync(catalogItem).GetAwaiter().GetResult();
            return catalogItem;
        }

        [NonAction]
        private string SaveImageToLocal(IFormFile image)
        {
            var imageName = $"{Guid.NewGuid()}_{image.FileName}";
            var dirName = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            var filePath = Path.Combine(dirName, imageName);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fs);
            }
            return $"/Images/{imageName}";
        }

        [NonAction]
        private async Task<string> SaveImageToCloudAsync(IFormFile image)
        {
            var imageName = $"{Guid.NewGuid()}_{image.FileName}";
            var tempFile = Path.GetTempFileName();
            using(FileStream fs = new FileStream(tempFile, FileMode.Create))
            {
              await  image.CopyToAsync(fs);
            }

            var imageFile = Path.Combine(Path.GetDirectoryName(tempFile), imageName);
            System.IO.File.Move(tempFile, imageFile);
            StorageAccountHelper storageHelper = new StorageAccountHelper();
            storageHelper.storageConnectionString = config.GetConnectionString("StorageConnection");
            var fileUri = await storageHelper.uploadFIlesToBlobAsnc(imageFile, "eshopimages");
            System.IO.File.Delete(imageFile);
            return fileUri;
        }


       [NonAction]
       private async Task<CatalogEntity> backUpToTableAsync(CatalogItem item)
        {
            StorageAccountHelper storageHelper = new StorageAccountHelper();
            storageHelper.tableConnectionString = config.GetConnectionString("TableStorageConnection");
            return await storageHelper.SaveToTableAsync(item);
        }
    }
}