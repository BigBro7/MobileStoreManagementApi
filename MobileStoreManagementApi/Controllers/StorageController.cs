using MobileStoreManagementApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MobileStoreManagementApi.Controllers
{
    [RoutePrefix("device")]
    public class StorageController : ApiController
    {
        private readonly MobileStoreManagementEntities db = new MobileStoreManagementEntities();

        [Route("addentry")]
        [HttpPost]
        public IHttpActionResult AddEntry(Store store)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if(store == null)
                {
                    return BadRequest();
                }
                if(store.BrandId == 0 || store.ModelId == 0)
                {
                    resp.Message = "Please provide Model and Brand";
                    goto ret;
                }
                if(store.RecievedPrice == 0)
                {
                    resp.Message = "Please enter price recieved";
                    goto ret;
                }

                Master_Storage storage = new Master_Storage();
                storage.BrandId = store.BrandId;
                storage.ModelId = store.ModelId;
                storage.Recieved_Date = DateTime.Now.Date;
                storage.Price = store.RecievedPrice;
                storage.Sold_Flag = false;

                db.Master_Storage.Add(storage);
                db.SaveChanges();

                resp.Status = 1;
                resp.Message = "Device stored successfully";
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
            ret:
            return Json(resp);
        }

        [Route("updateprice/{id}/{price}")]
        [HttpPost]
        public IHttpActionResult UpdatePrice(int id, double price)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                var stored = db.Master_Storage.Find(id);
                if(stored == null)
                {
                    resp.Message = "Not found";
                    goto ret;
                }

                stored.Price = price;
                db.SaveChanges();

                resp.Status = 1;
                resp.Message = "Price updated successfully";
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
        ret:
            return Json(resp);
        }

        [Route("delete/{id}")]
        [HttpDelete]
        public IHttpActionResult DeleteEntry(int id)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                var stored = db.Master_Storage.Where(s => s.Sold_Flag != true && s.Srno == id).FirstOrDefault();
                if (stored == null)
                {
                    resp.Message = "Either the device is sold of or it is not registered.";
                    goto ret;
                }

                db.Master_Storage.Remove(stored);
                db.SaveChanges();

                resp.Status = 1;
                resp.Message = "Entry deleted successfully";
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
        ret:
            return Json(resp);
        }

        [Route("get/{flag}")]
        [HttpGet]
        public IHttpActionResult GetSoldUnsold(string flag, int brandid = 0, int modelid = 0)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                string[] flagcontent = new string[] { "sold", "unsold" };
                if(!flagcontent.Contains(flag))
                {
                    return BadRequest();
                }

                IQueryable<Master_Storage> devicelist = db.Master_Storage;
                if (flag == "sold")
                {
                    devicelist = devicelist.Where(d => d.Sold_Flag == true);
                }
                else
                {
                    devicelist = devicelist.Where(x => x.Sold_Flag == false);
                }

                if (brandid != 0)
                {
                    devicelist = devicelist.Where(d => d.BrandId == brandid);
                    if (modelid != 0)
                    {
                        devicelist.Where(x => x.ModelId == modelid);
                    }
                }

                resp.Status = 1;
                resp.Message = "successfull";
                resp.Obj = devicelist.ToList();
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
            return Json(resp);
        }
    }
}
