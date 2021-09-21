using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    public class IncomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public IncomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {

            DateTime CurrentDate = DateTime.Now;

            var ObjectIncome = await _db.OrderDetails.Include(o => o.OrderHeader).Where(o => o.OrderId == o.OrderHeader.Id && o.OrderHeader.OrderDate.Date == CurrentDate.Date && o.OrderHeader.Status != SD.StatusCancelled && o.OrderHeader.PaymentStatus == SD.PaymentStatusApproved).ToListAsync();

            List<string> tempName = new List<string>();
            List<double> tempPrice = new List<double>();
            List<int> tempQuan = new List<int>();


            IncomeViewModel objIncome = new IncomeViewModel();

            if (ObjectIncome.Count > 0)
            {
                foreach (var item in ObjectIncome)
                {
                    tempName.Add(item.Name);
                    tempPrice.Add(item.Price);
                    tempQuan.Add(item.Count);
                    objIncome.IncomeMade += item.Price * item.Count;
                    objIncome.ItemSoldDay += item.Count;
                }

                var tempDup = FindDuplicates(tempName).ToList();
                if (tempDup.Count() > 0)
                {
                    for (int x = 0; x < tempDup.Count(); x++)
                    {
                        for (int y = 0; y < tempName.Count(); y++)
                        {
                            if (tempName[y] == tempDup[x].Item1 && tempPrice[y] == tempPrice[tempDup[x].Item2] && tempDup[x].Item2 != y)
                            {
                                tempQuan[y] += tempQuan[tempDup[x].Item2];
                                tempName[tempDup[x].Item2] = "";
                                tempPrice[tempDup[x].Item2] = 0;
                                tempQuan[tempDup[x].Item2] = 0;
                            }
                        }
                    }
                    tempName.RemoveAll(item => item == "");
                    tempPrice.RemoveAll(item => item == 0);
                    tempQuan.RemoveAll(item => item == 0);
                }

                objIncome.Name = tempName;
                objIncome.Price = tempPrice;
                objIncome.Quantity = tempQuan;
            }
            else
            {
                objIncome.Name = new List<string>(0);
                objIncome.Price = new List<double>(0);
                objIncome.Quantity = new List<int>(0);

            }


            return View(objIncome);
        }
        public static IEnumerable<Tuple<T, int>> FindDuplicates<T>(IEnumerable<T> data)
        {
            var hashSet = new HashSet<T>();
            int index = 0;
            foreach (var item in data)
            {
                if (hashSet.Contains(item))
                {
                    yield return Tuple.Create(item, index);
                }
                else
                {
                    hashSet.Add(item);
                }
                index++;
            }
        }
    }
}
    

