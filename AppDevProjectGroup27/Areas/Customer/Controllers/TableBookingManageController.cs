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

        public async Task<IActionResult> ARBooking()
        {
            List<TableDetailsVM> tableDetailsVM = new List<TableDetailsVM>();
            List<TableBookingHeader> tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending).OrderByDescending(t => t.SitInDate).ThenByDescending(t => t.SitInTime).ToListAsync();
            foreach (TableBookingHeader item in tableBookingHeaderList)
            {
                TableDetailsVM individual = new TableDetailsVM
                {
                    TableBookingHeader = item,
                    TableBookingDetails = await _db.TableBookingDetails.Where(t => t.TableBookingHeaderId == item.Id).ToListAsync()
                };

                tableDetailsVM.Add(individual);
            }

            return View(tableDetailsVM.OrderBy(t => t.TableBookingHeader.SitInDate).ThenBy(t => t.TableBookingHeader.SitInTime).ToList());
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


            return RedirectToAction(nameof(ARBooking));
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


            return RedirectToAction(nameof(ARBooking));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FilterByAR(DateTime? datepicker, DateTime? timepicker)
        {
            List<TableDetailsVM> tableDetailsVM = new List<TableDetailsVM>();
            List<TableBookingHeader> tableBookingHeaderList = new List<TableBookingHeader>();
            if ((datepicker == null && timepicker == null) || (datepicker == null && timepicker != null))
                return RedirectToAction(nameof(ARBooking));
            else if (datepicker != null && timepicker == null)
                tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending && t.SitInDate.Date == datepicker.Value.Date).OrderByDescending(t => t.SitInDate).ThenByDescending(t => t.SitInTime).ToListAsync();
            else if (datepicker != null && timepicker != null)
                tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending && t.SitInDate.Date == datepicker.Value.Date && t.SitInTime == timepicker.Value.TimeOfDay).OrderByDescending(t => t.SitInDate).ThenByDescending(t => t.SitInTime).ToListAsync();

            foreach (TableBookingHeader item in tableBookingHeaderList)
            {
                TableDetailsVM individual = new TableDetailsVM
                {
                    TableBookingHeader = item,
                    TableBookingDetails = await _db.TableBookingDetails.Where(t => t.TableBookingHeaderId == item.Id).ToListAsync()
                };

                tableDetailsVM.Add(individual);
            }

            return View(nameof(ARBooking), tableDetailsVM.OrderBy(t => t.TableBookingHeader.SitInDate).ThenBy(t => t.TableBookingHeader.SitInTime).ToList());

        }

        public async Task<IActionResult> ManageBooking()
        {
            List<TableDetailsVM> tableDetailsVM = new List<TableDetailsVM>();
            List<TableBookingHeader> tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved).OrderByDescending(t => t.SitInDate).ThenByDescending(t => t.SitInTime).ToListAsync();

            foreach (TableBookingHeader item in tableBookingHeaderList)
            {
                TableDetailsVM individual = new TableDetailsVM
                {
                    TableBookingHeader = item,
                    TableBookingDetails = await _db.TableBookingDetails.Where(t => t.TableBookingHeaderId == item.Id).ToListAsync()
                };

                tableDetailsVM.Add(individual);
            }

            return View(tableDetailsVM.OrderBy(t => t.TableBookingHeader.SitInDate).ThenBy(t => t.TableBookingHeader.SitInTime).ToList());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FilterBy(DateTime? datepicker, DateTime? timepicker)
        {
            List<TableDetailsVM> tableDetailsVM = new List<TableDetailsVM>();
            List<TableBookingHeader> tableBookingHeaderList = new List<TableBookingHeader>();
            if ((datepicker == null && timepicker == null) || (datepicker == null && timepicker != null))
                return RedirectToAction(nameof(ManageBooking));
            else if (datepicker != null && timepicker == null)
                tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved && t.SitInDate.Date == datepicker.Value.Date).OrderByDescending(t => t.SitInDate).ThenByDescending(t => t.SitInTime).ToListAsync();
            else if (datepicker != null && timepicker != null)
                tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved && t.SitInDate.Date == datepicker.Value.Date && t.SitInTime == timepicker.Value.TimeOfDay).OrderByDescending(t => t.SitInDate).ThenByDescending(t => t.SitInTime).ToListAsync();



            foreach (TableBookingHeader item in tableBookingHeaderList)
            {
                TableDetailsVM individual = new TableDetailsVM
                {
                    TableBookingHeader = item,
                    TableBookingDetails = await _db.TableBookingDetails.Where(t => t.TableBookingHeaderId == item.Id).ToListAsync()
                };

                tableDetailsVM.Add(individual);
            }

            return View(nameof(ManageBooking), tableDetailsVM.OrderBy(t => t.TableBookingHeader.SitInDate).ThenBy(t => t.TableBookingHeader.SitInTime).ToList());

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
                var tblDetailsObj = _db.TableBookingDetails.Include(t => t.TableBookingHeader).Where(t => t.TableBookingHeaderId == TableHeaderId).ToList();
                if (tblDetailsObj.Any())
                {
                    foreach (var item in tblDetailsObj)
                    {
                        TableTrack objT = _db.TableTrack.Include(t => t.Table).Where(t => t.DateTable.Date == item.TableBookingHeader.SitInDate.Date && t.TimeTable == item.TableBookingHeader.SitInTime && t.Table.SeatingName == item.TableName).FirstOrDefault();
                        if (objT != null)
                        {
                            objT.AmtAva += item.TableBooked;
                            _db.SaveChanges();
                        }
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
