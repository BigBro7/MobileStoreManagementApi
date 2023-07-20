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
    public class SellController : ApiController
    {
        
        private MobileStoreManagementEntities db = new MobileStoreManagementEntities();

        [Route("sell")]
        [HttpPost]
        public IHttpActionResult SellDevice(Sell sell)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if(sell == null)
                {
                    return BadRequest();
                }
                
                if (string.IsNullOrEmpty(sell.CustomerName))
                {
                    resp.Message = "Please provide customer name";
                    goto ret;
                }

                //get the product
                var product = db.Master_Storage.Find(sell.Id);
                if (product == null)
                {
                    resp.Message = "Not found";
                    goto ret;
                }
                if (product.Sold_Flag??false)
                {
                    resp.Message = "This product is already sold";
                    goto ret;
                }

                if (sell.SaidPrice == 0)
                    sell.SaidPrice = product.Price ?? 0;

                double discountamount = (sell.SaidPrice * sell.Discount) / 100;
                double sellprice = sell.SaidPrice - discountamount;

                product.Said_Price = sell.SaidPrice;
                product.Discount_Price = discountamount;
                product.Discount_Percentage = sell.Discount;
                product.Sold_Date = DateTime.Now.Date;
                product.Sold_Flag = true;
                product.Customer = sell.CustomerName;
                product.Sold_Price = sellprice;

                db.SaveChanges();

                resp.Status = 1;
                resp.Message = "Device updated successfully";

            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
        ret:
            return Json(resp);
        }

        [Route("bestprice")]
        [HttpGet]
        public IHttpActionResult BestPriceOfHandset(int brandid, int modelid)
        {
            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if(brandid == 0 || modelid == 0)
                {
                    resp.Message = "Please provide valid brandid and modelid";
                    goto ret;
                }

                var bestprice = db.Master_Storage.Where(x => x.BrandId == brandid && x.ModelId == modelid).Min(x => x.Sold_Price);
                if(bestprice == null)
                {
                    resp.Message = "Sorry! This model was never sold before";
                    goto ret;
                }

                resp.Status = 1;
                resp.Message = "Best price for this model is " + bestprice;
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
