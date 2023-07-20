using MobileStoreManagementApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MobileStoreManagementApi.Controllers
{
    [RoutePrefix("model")]
    public class ModelController : ApiController
    {
        private readonly MobileStoreManagementEntities db = new MobileStoreManagementEntities();

        [Route("addupdate")]
        [HttpPost]
        public IHttpActionResult AddUpdate(Master_Model model)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if (string.IsNullOrEmpty(model.ModelName))
                {
                    resp.Message = "Please enter modelname";
                    goto ret;
                }
                if(model.BrandId == 0)
                {
                    resp.Message = "Please provide brand";
                    goto ret;
                }

                if (model.Srno == 0)
                {
                    Master_Model newmodel = new Master_Model();
                    newmodel.ModelName = model.ModelName;
                    newmodel.BrandId = model.BrandId;

                    db.Master_Model.Add(newmodel);
                    db.SaveChanges();

                    resp.Status = 1;
                    resp.Message = "Model added successfully";
                }
                else
                {
                    var existmoddel = db.Master_Model.Find(model.Srno);
                    if (existmoddel == null)
                    {
                        resp.Message = "Not found";
                        goto ret;
                    }

                    existmoddel.BrandId = model.BrandId;
                    existmoddel.ModelName = model.ModelName;
                    db.SaveChanges();

                    resp.Status = 1;
                    resp.Message = "Model updated successfully";
                }
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
        ret:
            return Json(resp);
        }

        [Route("delete")]
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if (id == 0)
                {
                    resp.Message = "Please provide model to delete";
                    goto ret;
                }

                var existmodel = db.Master_Model.Find(id);
                if (existmodel == null)
                {
                    resp.Message = "Not found";
                    goto ret;
                }

                db.Master_Model.Remove(existmodel);
                db.SaveChanges();

                resp.Status = 1;
                resp.Message = "Model deleted successfully";
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }

        ret:
            return Json(resp);
        }

        [Route("get")]
        [HttpGet]
        public IHttpActionResult Get(int brandid, int modelid = 0)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if(brandid == 0)
                {
                    resp.Message = "Please provide brand";
                    goto ret;
                }
                List<ListItem> modellist = new List<ListItem>();
                var modelquery = db.Master_Model.Where(b => b.BrandId == brandid);
                if(modelid != 0)
                {
                    modelquery = modelquery.Where(m => m.Srno == modelid);
                }

                modellist = modelquery.Select(x => new ListItem { Id = x.Srno, Name = x.ModelName }).ToList();

                resp.Status = 1;
                resp.Message = "Successfull";
                resp.Obj = modellist;

            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
            ret:
            return Json(resp);
        }
    }
}
