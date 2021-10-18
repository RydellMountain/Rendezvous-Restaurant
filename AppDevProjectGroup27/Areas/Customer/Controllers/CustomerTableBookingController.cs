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
    [Authorize]
    [Area("Customer")]
    public class CustomerTableBookingController : Controller
    {
        private ApplicationDbContext _db;

        public CustomerTableBookingController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index()
        {
            CustomerBookingVM objCB = new CustomerBookingVM
            {
                TableTracks = null,
                ChosenTableId = 0,
                Quantity = 0
            };
            var TableObj = await _db.Table.AnyAsync();
            if (TableObj) objCB.TableList = await _db.Table.Where(t => t.Active == true).OrderBy(t=>t.SeatingName).Select(t => new SelectListItem
            {
                Text = t.SeatingName,
                Value = t.Id.ToString()
            }).ToListAsync();
            else
                objCB.TableList = new List<SelectListItem>();

            return View(objCB);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CustomerBookingVM objCB)
        {
            if (objCB.ChosenTableId <= 0 || objCB.DateChosen == null || objCB.TimeChosen == null)
                return RedirectToAction(nameof(Index));

            var FindIdInTableTrack = _db.TableTrack.Include(t => t.Table).Where(t => t.TableId == objCB.ChosenTableId
             && t.DateTable.Date == objCB.DateChosen.Value.Date
             && t.TimeTable == objCB.TimeChosen.Value.TimeOfDay);

            objCB.TableTracks = new TableTrack();

            if (FindIdInTableTrack.Any())
                objCB.TableTracks = await FindIdInTableTrack.FirstAsync();
            else
            {
                var GetTable = await _db.Table.FindAsync(objCB.ChosenTableId);
                int MaxQuan = GetTable.MaxTables;
                TableTrack tableTrack = new TableTrack
                {
                    TableId = objCB.ChosenTableId,
                    AmtAva = MaxQuan,
                    DateTable = objCB.DateChosen.Value.Date,
                    TimeTable = objCB.TimeChosen.Value.TimeOfDay
                };

                _db.TableTrack.Add(tableTrack);
                await _db.SaveChangesAsync();
                objCB.TableTracks = await _db.TableTrack.Include(t => t.Table).Where(t => t.Id == tableTrack.Id).Select(t => t).FirstAsync();
            }

            var TableObj = await _db.Table.AnyAsync();
            if (TableObj) objCB.TableList = await _db.Table.Where(t => t.Active == true).OrderBy(t=>t.SeatingName).Select(t => new SelectListItem
            {
                Text = t.SeatingName,
                Value = t.Id.ToString()
            }).ToListAsync();
            else
                objCB.TableList = new List<SelectListItem>();

            objCB.Quantity = 0;

            return View(objCB);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToTable(int? Quantity, int? TableTrackId)
        {
            if (Quantity.HasValue)
            {
                if (Quantity.Value <= 0)
                    return RedirectToAction(nameof(Index));
            }
            if (!Quantity.HasValue || !TableTrackId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }


            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var tableTrack = await _db.TableTrack.Include(t => t.Table).Where(t => t.Id == TableTrackId.Value).FirstOrDefaultAsync();

            if (tableTrack == null) return RedirectToAction(nameof(Index));

            TableBookingHeader objTBH = new TableBookingHeader
            {
                UserId = claim.Value,
                DateBookingMade = SharedMethods.GetDateTime(),
                SitInDate = tableTrack.DateTable,
                SitInTime = tableTrack.TimeTable,
                TableName = tableTrack.Table.SeatingName,
                TableBooked = Quantity.Value,
                Status = SD.TableStatusSubmitted,
                BookStatus = SD.BookTableStatusPending,
                ApprovedBy = "",
                RejectedBy = "",
                TimeApproved = null,
                TimeCheckOut = null,
                TimeRejected = null,
                TimeSitIn = null,
                Duration = ""
            };

            await _db.TableBookingHeader.AddAsync(objTBH);

            TableTrack objTT = await _db.TableTrack.FindAsync(tableTrack.Id);
            objTT.AmtAva -= Quantity.Value;

            await _db.SaveChangesAsync();

            return View("Confirm", objTBH);
        }
    }
}
