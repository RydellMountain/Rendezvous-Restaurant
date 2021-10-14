using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Customer.Controllers
{
    [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
    [Area("Customer")]
    public class TableBookingManageController : Controller
    {

        private ApplicationDbContext _db;
        public TableBookingManageController(ApplicationDbContext db)
        {
            _db = db;
        }


        // DONT FORGET: When table booking is involved customers cannot eat in one hour before final meal (Remove 9PM time)


        public async Task<IActionResult> ARBooking(string message = "")
        {
            List<TableBookingHeader> tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();

            TableARVM objTArm = new TableARVM { tableBookingHeaders = tableBookingHeaderList, StatusMessage = message };

            return View(objTArm);
        }
        public IActionResult Approve(int TableHeaderId)
        {
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.BookStatus = SD.BookTableStatusApproved;
            _db.SaveChanges();

            // Send Email To Customer that their Booking was approved
            //To Get Email:
            // var Email = _db.ApplicationUser.Find(tableHeader.UserId).Email;
            // End of Get Email (Please check if this code works first

            string Message = "Table Booking Approved";

            return RedirectToAction(nameof(ARBooking), new { message = Message });
        }
        public IActionResult Reject(int TableHeaderId)
        {
            // Add Quantity back to Table -  if Available
            if (!UpdateQuantity(TableHeaderId))
            {
                return NotFound();
                // Or Can Return Error Message
            }
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.BookStatus = SD.BookTableStatusRejected;
            _db.SaveChanges();

            // Send Email To Customer that their Booking was rejected
            //To Get Email:
            // var Email = _db.ApplicationUser.Find(tableHeader.UserId).Email;
            // End of Get Email (Please check if this code works first

            return RedirectToAction(nameof(ARBooking), new { message = "Table Booking Rejected" });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FilterByAR(DateTime? datepicker, DateTime? timepicker)
        {
            List<TableBookingHeader> tableBookingHeaderList = new List<TableBookingHeader>();
            if ((datepicker == null && timepicker == null) || (datepicker == null && timepicker != null))
                return RedirectToAction(nameof(ARBooking));
            else if (datepicker != null && timepicker == null)
                tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending && t.SitInDate.Date == datepicker.Value.Date).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();
            else if (datepicker != null && timepicker != null)
                tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending && t.SitInDate.Date == datepicker.Value.Date && t.SitInTime == timepicker.Value.TimeOfDay).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();


            TableARVM objTArm = new TableARVM { tableBookingHeaders = tableBookingHeaderList, StatusMessage = "" };
            return View(nameof(ARBooking), objTArm);

        }

        public async Task<IActionResult> ManageBooking()
        {
            List<TableBookingHeader> tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();
            return View(tableBookingHeaderList);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FilterBy(DateTime? datepicker, DateTime? timepicker)
        {
            List<TableBookingHeader> tableBookingHeaderList = new List<TableBookingHeader>();
            if ((datepicker == null && timepicker == null) || (datepicker == null && timepicker != null))
                return RedirectToAction(nameof(ManageBooking));
            else if (datepicker != null && timepicker == null)
                tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved && t.SitInDate.Date == datepicker.Value.Date).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();
            else if (datepicker != null && timepicker != null)
                tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved && t.SitInDate.Date == datepicker.Value.Date && t.SitInTime == timepicker.Value.TimeOfDay).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();


            return View(nameof(ManageBooking), tableBookingHeaderList);

        }


        public IActionResult StartSitIn(int TableHeaderId)
        {
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.Status = SD.TableStatusStart;
            _db.SaveChanges();
            return RedirectToAction(nameof(ManageBooking));
        }

        public IActionResult CompleteSitIn(int TableHeaderId)
        {
            // Add Quantity back to Table Available
            if (!UpdateQuantity(TableHeaderId))
            {
                return NotFound();
                // Or Can Return Error Message
            }

            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.Status = SD.TableStatusCompleted;
            _db.SaveChanges();


            //Email Logic to thank user for eating with us

            return RedirectToAction(nameof(ManageBooking));
        }

        public IActionResult CancelBooking(int TableHeaderId)
        {
            // Add Quantity back to Table Available
            if (!UpdateQuantity(TableHeaderId))
            {
                return NotFound();
                // Or Can Return Error Message
            }
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.Status = SD.TableStatusCancelled;
            _db.SaveChanges();
            return RedirectToAction(nameof(ManageBooking));
        }


        public bool UpdateQuantity(int TableHeaderId)
        {
            try
            {
                var tblDetailsObj = _db.TableBookingHeader.Where(t => t.Id == TableHeaderId).FirstOrDefault();
                if (tblDetailsObj != null)
                {
                    TableTrack objT = _db.TableTrack.Include(t => t.Table).Where(t => t.DateTable.Date == tblDetailsObj.SitInDate.Date && t.TimeTable == tblDetailsObj.SitInTime && t.Table.SeatingName == tblDetailsObj.TableName).FirstOrDefault();
                    if (objT != null)
                    {
                        objT.AmtAva += tblDetailsObj.TableBooked;
                        if (objT.AmtAva > objT.Table.MaxTables)
                            objT.AmtAva = objT.Table.MaxTables;

                        _db.SaveChanges();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
