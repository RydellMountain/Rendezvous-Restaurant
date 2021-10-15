using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TableController : Controller
    {

        public readonly ApplicationDbContext _db;
        public TableController(ApplicationDbContext db)
        {
            _db = db;
        }


        [TempData]
        public string StatusMessage { get; set; }


        public async Task<IActionResult> Index()
        {
            return View(await _db.Table.ToListAsync());
        }

        //Get - Create
        public IActionResult Create()
        {
            TableCreateVM objT = new TableCreateVM();
            return View(objT);
        }

        //Post - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TableCreateVM objTable)
        {
            if (objTable.Table.SeatingName == null)
            {
                ModelState.AddModelError("Table.SeatingName", "Please enter a number");
                return View(objTable);
            }
            if (ModelState.IsValid)
            {
                string TableName = objTable.Table.SeatingName + " Seater";
                var DoesTableAlreadyExist = _db.Table.Where(t => t.SeatingName == TableName);
                if (DoesTableAlreadyExist.Any())
                {
                    StatusMessage = "Error : A " + TableName + " Table already exists.";
                }
                else
                {
                    objTable.Table.SeatingName += " Seater";
                    _db.Table.Add(objTable.Table);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

            }
            objTable.StatusMessage = StatusMessage;
            return View(objTable);
        }

        //Get-Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var table = await _db.Table.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            var integerValue = GetIntValue(table.SeatingName);

            if (integerValue == 0) return NotFound();

            table.SeatingName = integerValue.ToString();
            TableEditVM objT = new TableEditVM { Table = table, OldMaxValue = table.MaxTables };

            return View(objT);
        }

        /*
         ADMIN:
            ID: 1
            Name: 1 Seater     >> 1 Seater 
            Max Value: 5 Tables
            Available: 2 Tables
            ID: 2
            Name: 2 Seater
            Max Value: 4 Tables
            Available: 1 Tables
        New:
            Name: 1 Seater
            Max Value: 3 Tables
            Available: 0 Tables
         
            AVA --- NEW MAX VALUE  - OLD MAX VALUE
                        3          -       5        =   -2
            AVAILABLE += AVA // -2;       ----->>>> 0
           
         
         */

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(TableEditVM objTable)
        {
            if (ModelState.IsValid)
            {
                if (objTable.Table.MaxTables != objTable.OldMaxValue)
                {
                    int SubtractTab = objTable.Table.MaxTables - objTable.OldMaxValue;

                    var TableTrackObj = await _db.TableTrack.Include(t => t.Table).Where(t => t.TableId == objTable.Table.Id).ToListAsync();

                    if (TableTrackObj.Any())
                    {
                        foreach (var item in TableTrackObj)
                        {
                            TableTrack objTT = await _db.TableTrack.FindAsync(item.Id);
                            objTT.AmtAva += SubtractTab;
                            if (objTT.AmtAva < 0) objTT.AmtAva = 0;
                        }
                    }
                }

                var TableFromDb = await _db.Table.FindAsync(objTable.Table.Id);
                TableFromDb.MaxTables = objTable.Table.MaxTables;
                TableFromDb.Active = objTable.Table.Active;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(objTable);
        }

        //Get-Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _db.Table.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }
            return View(table);

        }

        //Post-Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var table = await _db.Table.FindAsync(id);
            if (table == null)
            {
                return View();
            }

            // Send Email to All pending and approved bookings for specific item that is being deleted
            // Inform Customers that the table is no longer available etc.

            /*
             Following code is to remove all table tracks in TableTrack that is linked to the table being
            deleted.
             */
            var CheckTableTrack = await _db.TableTrack.Where(t => t.TableId == id).ToListAsync();
            if (CheckTableTrack.Any())
            {
                foreach (var item in CheckTableTrack)
                {
                    TableTrack tableTrack = await _db.TableTrack.FindAsync(item.Id);
                    _db.TableTrack.Remove(tableTrack);
                }
            }

            /*
            Following code is to cancel/reject all bookings that have the soon to be deleted table name
            */

            //Reject Pending Ones:
            var CheckTableHeaderPending = await _db.TableBookingHeader.Where(t => t.TableName == table.SeatingName && t.Status != SD.TableStatusCancelled && t.BookStatus == SD.BookTableStatusPending && t.SitInDate.Date > DateTime.Now.Date).ToListAsync();
            if (CheckTableHeaderPending.Any())
            {
                foreach (var item in CheckTableHeaderPending)
                {
                    TableBookingHeader tableHeader = _db.TableBookingHeader.Find(item.Id);
                    tableHeader.BookStatus = SD.BookTableStatusRejected;
                    tableHeader.TimeRejected = DateTime.Now;

                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

                    tableHeader.RejectedBy = StaffName;
                    // Send Email saying their booking was rejected because table is unavailable
                }
            }

            //Cancel Approved Ones:
            var CheckTableHeaderApproved = await _db.TableBookingHeader.Where(t => t.TableName == table.SeatingName && t.BookStatus == SD.BookTableStatusApproved && t.Status == SD.TableStatusSubmitted && t.SitInDate.Date > DateTime.Now.Date).ToListAsync();
            if (CheckTableHeaderApproved.Any())
            {
                foreach (var item in CheckTableHeaderApproved)
                {
                    TableBookingHeader tableHeader = _db.TableBookingHeader.Find(item.Id);
                    tableHeader.Status = SD.TableStatusCancelled;
                    tableHeader.TimeRejected = DateTime.Now;
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

                    tableHeader.RejectedBy = StaffName;
                    // Send Email saying their booking was cancelled because table is unavailable
                }
            }




            _db.Table.Remove(table);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //GET - Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var table = await _db.Table.FindAsync(id);
            if (table == null)
                return NotFound();

            return View(table);
        }

        public int GetIntValue(string Temp)
        {
            var integerValue = 0;
            var numericString = new string(Temp.Where(x => char.IsDigit(x)).ToArray());
            int.TryParse(numericString, out integerValue);
            return integerValue;

        }
    }

}
