using MobileStoreManagementApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MobileStoreManagementApi.Controllers
{
    [RoutePrefix("brand")]
    public class BrandController : ApiController
    {
        private readonly MobileStoreManagementEntities db = new MobileStoreManagementEntities();

        [Route("addupdate")]
        [HttpPost]
        public IHttpActionResult AddUpdate(string brandname, int brandid = 0)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if (string.IsNullOrEmpty(brandname))
                {
                    resp.Message = "Please enter brandname";
                    goto ret;
                }

                if(brandid == 0)
                {
                    Master_Brand brand = new Master_Brand();
                    brand.BrandName = brandname;

                    db.Master_Brand.Add(brand);
                    db.SaveChanges();

                    resp.Status = 1;
                    resp.Message = "Brand added successfully";
                }
                else
                {
                    var existbrand = db.Master_Brand.Find(brandid);
                    if (existbrand == null)
                    {
                        resp.Message = "Not found";
                        goto ret;
                    }

                    existbrand.BrandName = brandname;
                    db.SaveChanges();

                    resp.Status = 1;
                    resp.Message = "Brand updated successfully";
                }
            }
            catch(Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
            ret:
            return Json(resp);
        }

        [Route("delete")]
        [HttpDelete]
        public IHttpActionResult Delete(int brandid)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if(brandid == 0)
                {
                    resp.Message = "Please provide brand to delete";
                    goto ret;
                }

                var existbrand = db.Master_Brand.Find(brandid);
                if(existbrand == null)
                {
                    resp.Message = "Not found";
                    goto ret;
                }

                db.Master_Brand.Remove(existbrand);
                db.SaveChanges();

                resp.Status = 1;
                resp.Message = "Brand deleted successfully";
            }
            catch(Exception e)
            {
                resp.Message = e.Message+e.StackTrace + e.InnerException;
            }
            
            ret:
            return Json(resp);  
        }

        [Route("get")]
        [HttpGet]
        public IHttpActionResult Get(int brandid = 0)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                List<ListItem> brandlist = new List<ListItem>();
                if (brandid == 0)
                {
                     brandlist =  db.Master_Brand.Select(s => new ListItem { Id = s.Srno, Name = s.BrandName }).ToList();
                }
                else
                {
                    brandlist = db.Master_Brand.Where(x => x.Srno == brandid).Select(x => new ListItem { Id = x.Srno, Name = x.BrandName}).ToList(); 
                }

                resp.Status = 1;
                resp.Message = "Successfull";
                resp.Obj = brandlist;

            }
            catch(Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
            return Json(resp);
        }
    }
}
