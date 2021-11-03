using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class TableBookingHistoryController : Controller
    {
        private ApplicationDbContext _db;
        private int PageSize = 2;

        public TableBookingHistoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Index(int productPage = 1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<TableBookingHeader> objTableHeader = await _db.TableBookingHeader.Include(t => t.ApplicationUser)
                .Where(t => t.UserId == claims.Value).ToListAsync();

            var count = objTableHeader.Count;

            TableBookingHistVM objTBHVM = new TableBookingHistVM();

            objTBHVM.tableBookingHeaders = objTableHeader.OrderByDescending(t => t.Id)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize).ToList();

            objTBHVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemPerPage = PageSize,
                TotalItems = count,
                UrlParam = "/Customer/TableBookingHistory/Index?productPage=:"
            };

            return View(objTBHVM);
        }



        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public IActionResult CusTableBookingHis()
        {
            return View(GetCusTableBookingVM(SD.TableStatusCompleted, SD.BookTableStatusApproved, SharedMethods.GetDateTime().Date));
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CusTableBookingHis(string Status, string BookStatus, DateTime? datepicker)
        {
            return View(GetCusTableBookingVM(Status, BookStatus, datepicker));
        }


        public CusTableBookingHistoryVM GetCusTableBookingVM(string Status , string BookStatus , DateTime? dateChosen)
        {
            CusTableBookingHistoryVM cusTableBookingHistoryVM = new CusTableBookingHistoryVM()
            {
                TableBookings = new List<TableBookingHeader>(),
                Status = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = SD.TableStatusSubmitted, Value = SD.TableStatusSubmitted },
                    new SelectListItem() { Text = SD.TableStatusStart,  Value = SD.TableStatusStart },
                    new SelectListItem() { Text = SD.TableStatusCompleted, Value = SD.TableStatusCompleted },
                    new SelectListItem() { Text = SD.TableStatusCancelled, Value = SD.TableStatusCancelled }
                },
                BookStatus = new  List<SelectListItem>()
                {
                    new SelectListItem() { Text = SD.BookTableStatusApproved , Value = SD.BookTableStatusApproved },
                    new SelectListItem() { Text = SD.BookTableStatusRejected, Value = SD.BookTableStatusRejected },
                    new SelectListItem() { Text = SD.BookTableStatusPending, Value = SD.BookTableStatusPending }
                },
                CurrentDate = SharedMethods.GetDateTime().Date,
                StatusChosen = Status,
                BookStatusChosen = BookStatus
            };

            var TableBookingTbl = _db.TableBookingHeader.Select(t => t.DateBookingMade);
            if (TableBookingTbl.Any())
                cusTableBookingHistoryVM.EarliestDate = TableBookingTbl.Min().Date;
            else
                cusTableBookingHistoryVM.EarliestDate = SharedMethods.GetDateTime().Date;

            List<TableBookingHeader> tableBookingHeadersList = new List<TableBookingHeader>();

            if (!string.IsNullOrEmpty(Status) && !string.IsNullOrEmpty(BookStatus) && dateChosen != null)
                tableBookingHeadersList = _db.TableBookingHeader.Include(t=>t.ApplicationUser).Where(t => t.Status == Status && t.BookStatus == BookStatus && t.DateBookingMade.Date == dateChosen.Value.Date).ToList();
            else if (!string.IsNullOrEmpty(Status) && string.IsNullOrEmpty(BookStatus) && dateChosen == null)
                tableBookingHeadersList = _db.TableBookingHeader.Include(t => t.ApplicationUser).Where(t => t.Status == Status).ToList();
            else if (string.IsNullOrEmpty(Status) && !string.IsNullOrEmpty(BookStatus) && dateChosen == null)
                tableBookingHeadersList = _db.TableBookingHeader.Include(t => t.ApplicationUser).Where(t => t.BookStatus == BookStatus).ToList();
            else if (string.IsNullOrEmpty(Status) && string.IsNullOrEmpty(BookStatus) && dateChosen != null)
                tableBookingHeadersList = _db.TableBookingHeader.Include(t => t.ApplicationUser).Where(t => t.DateBookingMade.Date == dateChosen.Value.Date).ToList();
            else if (!string.IsNullOrEmpty(Status) && !string.IsNullOrEmpty(BookStatus) && dateChosen == null)
                tableBookingHeadersList = _db.TableBookingHeader.Include(t => t.ApplicationUser).Where(t => t.Status == Status && t.BookStatus == BookStatus).ToList();
            else if (!string.IsNullOrEmpty(Status) && string.IsNullOrEmpty(BookStatus) && dateChosen != null)
                tableBookingHeadersList = _db.TableBookingHeader.Include(t => t.ApplicationUser).Where(t => t.Status == Status && t.DateBookingMade.Date == dateChosen.Value.Date).ToList();
            else if (string.IsNullOrEmpty(Status) && !string.IsNullOrEmpty(BookStatus) && dateChosen != null)
                tableBookingHeadersList = _db.TableBookingHeader.Include(t => t.ApplicationUser).Where(t => t.BookStatus == BookStatus && t.DateBookingMade.Date == dateChosen.Value.Date).ToList();
            else
            {
                Status = SD.TableStatusCompleted;
                BookStatus = SD.BookTableStatusApproved;
                dateChosen = SharedMethods.GetDateTime().Date;
                tableBookingHeadersList = _db.TableBookingHeader.Include(t => t.ApplicationUser).Where(t => t.Status == SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved && t.DateBookingMade == SharedMethods.GetDateTime().Date).ToList();
            }

            cusTableBookingHistoryVM.TableBookings = tableBookingHeadersList.OrderByDescending(t => t.Id).ToList();
            cusTableBookingHistoryVM.DisplayDate = dateChosen;


            return cusTableBookingHistoryVM;

        }
    }
}
