using MobileStoreManagementApi.Models;
using OfficeOpenXml;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.Ajax.Utilities;
using System.Collections.Generic;

namespace MobileStoreManagementApi.Controllers
{
    [RoutePrefix("report")]
    public class ReportsController : ApiController
    {
        private MobileStoreManagementEntities db = new MobileStoreManagementEntities();

        [HttpGet]
        [Route("monthlysales")]
        public HttpResponseMessage MonthlyReport(string from, string to)
        {
            HttpResponseMessage response = Request.CreateResponse(200);            

            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                CultureInfo enUs = new CultureInfo("en-US");
                if (!DateTime.TryParseExact(from, "dd-MM-yyyy", enUs, DateTimeStyles.None, out DateTime fromdate))
                {
                    resp.Message = "Invalid date format! should of the format 'dd-MM-yyyy'";
                    goto ret;
                }
                if (!DateTime.TryParseExact(to, "dd-MM-yyyy", enUs, DateTimeStyles.None, out DateTime todate))
                {
                    resp.Message = "Invalid date format! should of the format 'dd-MM-yyyy'";
                    goto ret;
                }

                int totaldays = (todate - fromdate).Days;
                if (totaldays < 0)
                {
                    resp.Message = "Please provide valid date range";
                    goto ret;
                }

                //get all the transaction on this date
                var records = (from s in db.Master_Storage
                               where s.Sold_Date >= fromdate && s.Sold_Date <= todate && s.Sold_Flag == true
                               select new Sale
                               {
                                   Date = s.Sold_Date ?? DateTime.Now,
                                   Discount = s.Discount_Price ?? 0,
                                   Amount = s.Sold_Price ?? 0
                               });                

                //--------------------------------XL Report----------------------------------                
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();

                // name of the sheet
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.TabColor = System.Drawing.Color.Black;
                workSheet.DefaultRowHeight = 12;
                workSheet.Cells[1, 3].Value = "Monthly sales report(" + from + " - " + to + ")";

                workSheet.Cells[3, 2].Value = "Date";
                workSheet.Cells[3, 3].Value = "Unit Quantity";
                workSheet.Cells[3, 4].Value = "Total Disount";
                workSheet.Cells[3, 5].Value = "Total amount";
                int rowindex = 5;

                do
                {
                    var tempdate = fromdate;

                    var daterecords = records.Where(r => r.Date == tempdate).ToList();

                    int unitquantity = daterecords.Count();
                    double totaldiscount = 0;
                    double totalamount = 0;

                    foreach (var item in daterecords)
                    {
                        totaldiscount += item.Discount;
                        totalamount += item.Amount;
                    }

                    workSheet.Cells[rowindex, 2].Value = tempdate.ToString("dd-MM-yyyy");
                    workSheet.Cells[rowindex, 3].Value = daterecords.Count();
                    workSheet.Cells[rowindex, 4].Value = totaldiscount;
                    workSheet.Cells[rowindex, 5].Value = totalamount;

                    rowindex++;
                    fromdate = fromdate.AddDays(1);

                } while (fromdate <= todate);

                var datastream = new MemoryStream(excel.GetAsByteArray());
                response.Content = new StreamContent(datastream);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "MonthlySalesReport.xlsx";
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

                return response;
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
        ret:
            string jsonstring = JsonConvert.SerializeObject(resp);
            response.Content = new StringContent(jsonstring);
            return response;
        }

        [HttpGet]
        [Route("Brandwise")]
        public HttpResponseMessage BrandwiseReport(string from, string to, int brandid)
        {
            HttpResponseMessage response = Request.CreateResponse(200);

            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                CultureInfo enUs = new CultureInfo("en-US");
                if (!DateTime.TryParseExact(from, "dd-MM-yyyy", enUs, DateTimeStyles.None, out DateTime fromdate))
                {
                    resp.Message = "Invalid date format! should of the format 'dd-MM-yyyy'";
                    goto ret;
                }
                if (!DateTime.TryParseExact(to, "dd-MM-yyyy", enUs, DateTimeStyles.None, out DateTime todate))
                {
                    resp.Message = "Invalid date format! should of the format 'dd-MM-yyyy'";
                    goto ret;
                }
                if(brandid == 0)
                {
                    resp.Message = "Please provide Brand";
                    goto ret;
                }

                int totaldays = (todate - fromdate).Days;
                if (totaldays < 0)
                {
                    resp.Message = "Please provide valid date range";
                    goto ret;
                }

                //get brand
                var brandname = db.Master_Brand.Where(x => x.Srno == brandid).FirstOrDefault().BrandName;

                //get the details
                var records = (from s in db.Master_Storage
                               where s.BrandId == brandid && s.Sold_Flag == true
                               && s.Sold_Date >= fromdate && s.Sold_Date <= todate
                               join b in db.Master_Brand on s.BrandId equals b.Srno
                               join m in db.Master_Model on s.ModelId equals m.Srno
                               select new Brandwise
                               {
                                   Date = s.Sold_Date ?? DateTime.MinValue,
                                   Model = m.ModelName,
                                   TotalDiscount = s.Discount_Price ?? 0,
                                   TotalAmount = s.Sold_Price ?? 0,
                               });


                //--------------------------------XL Report----------------------------------                
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();

                // name of the sheet
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.TabColor = System.Drawing.Color.Black;
                workSheet.DefaultRowHeight = 12;
                workSheet.Cells[1, 3].Value = "Monthly sales for Brand " + brandname + "(" + from + " to " + to + ")";

                workSheet.Cells[3, 2].Value = "Date";
                workSheet.Cells[3, 3].Value = "Model";
                workSheet.Cells[3, 4].Value = "Unit Quantity";
                workSheet.Cells[3, 5].Value = "Total Discount";
                workSheet.Cells[3, 6].Value = "Total Amount";
                int rowindex = 5;

                do
                {
                    var tempdate = fromdate;

                    var daterecords = records.Where(r => r.Date == tempdate).ToList();
                    var distinctmodels = daterecords.Select(x => x.Model).Distinct().ToList();
                    double totaldiscount = 0;
                    double totalamount = 0;

                    workSheet.Cells[rowindex, 2].Value = tempdate.ToString("dd-MM-yyyy");
                    foreach(var model in distinctmodels)
                    {
                        //get all the models count and discount
                        var modelrecords = daterecords.Where(x => x.Model == model);

                        workSheet.Cells[rowindex, 3].Value = model;
                        workSheet.Cells[rowindex, 4].Value = modelrecords.Count();
                        workSheet.Cells[rowindex, 5].Value = modelrecords.Sum(x=> x.TotalDiscount);
                        totaldiscount += modelrecords.Sum(x => x.TotalDiscount);
                        workSheet.Cells[rowindex, 6].Value = modelrecords.Sum(x => x.TotalAmount);
                        totalamount += modelrecords.Sum(x => x.TotalAmount);
                        rowindex++;
                    }
                    workSheet.Cells[rowindex, 5].Value = totaldiscount;
                    workSheet.Cells[rowindex, 6].Value = totalamount;

                    rowindex++;
                    fromdate = fromdate.AddDays(1);

                } while (fromdate <= todate);

                var datastream = new MemoryStream(excel.GetAsByteArray());
                response.Content = new StreamContent(datastream);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "MonthlySalesReport.xlsx";
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

                return response;
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }
        ret:
            string jsonstring = JsonConvert.SerializeObject(resp);
            response.Content = new StringContent(jsonstring);
            return response;
        }

        [HttpGet]
        [Route("ProfitLoss")]
        public HttpResponseMessage ProfitLossReport(int year)
        {
            HttpResponseMessage response = Request.CreateResponse(200);

            ApiResponse resp = new ApiResponse()
            {
                Status = 0
            };
            try
            {
                if(year == 0)
                {
                    resp.Message = "Please provide valid year";
                    goto ret;
                }

                //get the records
                List<Profitloss> list = new List<Profitloss>();
                for(int i = 1; i <= 12; i++)
                {
                    var monthlist = (from s in db.Master_Storage
                                     where s.Sold_Flag == true
                                     && (s.Sold_Date ?? DateTime.MaxValue).Month == i
                                     select new
                                     {
                                         Month = s.Sold_Date,
                                         Discount = s.Discount_Price,
                                         Price = s.Sold_Price,
                                         RecievedPrice = s.Price
                                     }).ToList() ;

                    Profitloss element = new Profitloss();
                    element.Month = new DateTime(2023,i,1).ToString("MMM");
                    element.TotalDiscount = (double)monthlist.Sum(x => x.Discount);
                    element.UnitQuantity = monthlist.Count();
                    element.TotalPrice = (double)monthlist.Sum((x) => x.Price);
                    element.TotalReceivedPrice = (double)monthlist.Sum(s => s.RecievedPrice);

                    list.Add(element);                    
                }

                //--------------------------------XL Report----------------------------------                
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();

                // name of the sheet
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.TabColor = System.Drawing.Color.Black;
                workSheet.DefaultRowHeight = 12;
                workSheet.Cells[1, 3].Value = "Profit/Loss report for the year" + year;

                workSheet.Cells[3, 2].Value = "Month";
                workSheet.Cells[3, 3].Value = "Unit quantity";
                workSheet.Cells[3, 4].Value = "Total Recieved Price";
                workSheet.Cells[3, 5].Value = "Total Discount Price";
                workSheet.Cells[3, 6].Value = "Total Sold Price";
                workSheet.Cells[3, 7].Value = "Profit/Loss";

                int rowindex = 5;

                foreach(var item in list)
                {
                    workSheet.Cells[rowindex, 2].Value = item.Month;
                    workSheet.Cells[rowindex, 3].Value = item.UnitQuantity;
                    workSheet.Cells[rowindex, 4].Value = item.TotalReceivedPrice;
                    workSheet.Cells[rowindex, 5].Value = item.TotalDiscount;
                    workSheet.Cells[rowindex, 6].Value = item.TotalPrice;

                    //profit/loss calculation
                    var profitloss = (item.TotalReceivedPrice - item.TotalPrice);
                    workSheet.Cells[rowindex, 7].Value = profitloss;

                    rowindex++;
                }
                

                var datastream = new MemoryStream(excel.GetAsByteArray());
                response.Content = new StreamContent(datastream);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "MonthlySalesReport.xlsx";
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

                return response;
            }
            catch (Exception e)
            {
                resp.Message = e.Message + e.StackTrace + e.InnerException;
            }

        ret:
            string jsonstring = JsonConvert.SerializeObject(resp);
            response.Content = new StringContent(jsonstring);
            return response;
        }
    }
}
