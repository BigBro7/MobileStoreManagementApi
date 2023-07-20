using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileStoreManagementApi.Models
{
    public class ApiResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public object Obj { get; set; }
    }

    public class ListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Store
    {
        public int BrandId { get; set; }
        public int ModelId { get; set; }
        public float RecievedPrice { get; set; }
    }

    public class Sell
    {
        public int Id { get; set; }
        public double SaidPrice { get; set; }
        public double Discount { get; set; }
        public string CustomerName { get; set; }
    }

    public class Monthly
    {
        public DateTime Date { get; set; }
        public int UnitQuantity { get; set; }
        public double TotalDiscount { get; set; }
        public double TotalAmount { get; set; }
    }
    //---------------------------------------------------REPORT MODELS------------------------------------


    public class Sale
    {
        public DateTime Date { get; set; }
        public double Discount { get; set; }
        public double Amount { get; set; }
    }

    public class Brandwise
    {
        public DateTime Date { get; set; }
        public string Model { get; set; }
        public int UnitQuantity { get; set; }
        public double TotalDiscount { get; set; }
        public double TotalAmount { get; set; }

    }

    public class Profitloss
    {
        public string Month { get; set; }
        public int UnitQuantity { get; set; }
        public double TotalDiscount { get; set; }
        public double TotalPrice { get; set; }
        public double TotalReceivedPrice { get; set; }
    }
}