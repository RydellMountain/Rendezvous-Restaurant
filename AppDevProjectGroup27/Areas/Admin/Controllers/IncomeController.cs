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
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class IncomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public IncomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            DateTime CurrentDate = SharedMethods.GetDateTime();
            return View(GetListIncome(CurrentDate));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(DateTime? datepicker)
        {
            if (datepicker == null)
                return RedirectToAction(nameof(Index));

            return View(GetListIncome(datepicker.Value));
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
        public IncomeVM GetListIncome(DateTime DateSearch)
        {
            var ObjectIncome = _db.OrderDetails.Include(o => o.OrderHeader).Where(o => o.OrderId == o.OrderHeader.Id && o.OrderHeader.OrderDate.Date == DateSearch.Date && o.OrderHeader.Status != SD.StatusCancelled && o.OrderHeader.PaymentStatus == SD.PaymentStatusApproved).ToList();

            List<string> tempName = new List<string>();
            List<double> tempPrice = new List<double>();
            List<int> tempQuan = new List<int>();


            IncomeVM objIncome = new IncomeVM();

            objIncome.DisplayDate = DateSearch.Date;

            var OrderHeaderItem = _db.OrderHeader.Where(o => o.Status != SD.StatusCancelled && o.PaymentStatus == SD.PaymentStatusApproved).Select(o => o.OrderDate);
            if (OrderHeaderItem.Any())
                objIncome.EarliestDay = OrderHeaderItem.Min().Date;
            else
                objIncome.EarliestDay = SharedMethods.GetDateTime().Date;

            if (ObjectIncome.Any())
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


                List<ItemVM> objItemVM = new List<ItemVM>();

                for (int x = 0; x < tempName.Count(); x++)
                {
                    objItemVM.Add(new ItemVM { ItemName = tempName[x], ItemPrice = tempPrice[x], ItemQuan = tempQuan[x] });
                }

                objIncome.IncomeList = objItemVM.OrderBy(x => x.ItemName);
            }
            else
            {
                objIncome.IncomeList = new List<ItemVM>(0);

            }

            objIncome.CurrentDate = SharedMethods.GetDateTime().Date;

            return objIncome;
        }
    }
}
    

