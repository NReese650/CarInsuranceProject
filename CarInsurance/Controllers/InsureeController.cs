using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CarInsurance.Models;

namespace CarInsurance.Controllers
{
    public class InsureeController : Controller
    {
        private InsuranceEntities db = new InsuranceEntities();

        private int CalculateQuote(Insuree insuree)
        {
            int baseRate = 50;

            // Age calculation
            var age = DateTime.Today.Year - insuree.DateOfBirth.Year;
            if (age <= 18)
            {
                baseRate += 100;
            }
            else if (age > 18 && age <= 25)
            {
                baseRate += 50;
            }
            else
            {
                baseRate += 25;
            }

            // Car year calculation
            if (insuree.CarYear < 2000 || insuree.CarYear > 2015)
            {
                baseRate += 25;
            }

            // Car make and model calculation
            if (insuree.CarMake.Equals("Porsche", StringComparison.OrdinalIgnoreCase))
            {
                baseRate += 25;
                if (insuree.CarModel.Equals("911 Carrera", StringComparison.OrdinalIgnoreCase))
                {
                    baseRate += 25;
                }
            }

            // Speeding tickets
            baseRate += insuree.SpeedingTickets * 10;

            // DUI check
            if (insuree.DUI)
            {
                baseRate = (int)(baseRate * 1.25);
            }

            // Full coverage check
            if (insuree.CoverageType) // Assuming CoverageType is a boolean
            {
                baseRate = (int)(baseRate * 1.5);
            }

            return baseRate;
        }


        // GET: Insuree
        public ActionResult Index()
        {
            return View(db.Insurees.ToList());
        }

        // GET: Insuree/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Insuree insuree = db.Insurees.Find(id);
            if (insuree == null)
            {
                return HttpNotFound();
            }
            return View(insuree);
        }

        // GET: Insuree/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Insuree/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,EmailAddress,DateOfBirth,CarYear,CarMake,CarModel,DUI,SpeedingTickets,CoverageType,Quote")] Insuree insuree)
        {
            if (ModelState.IsValid)
            {
                insuree.Quote = CalculateQuote(insuree);
                db.Insurees.Add(insuree);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = insuree.Id });
            }

            return View(insuree);
        }

        public ActionResult QuoteResult(int id)
        {
            var insuree = db.Insurees.Find(id);
            if (insuree == null)
            {
                return HttpNotFound();
            }
            return View(insuree);
        }

        // GET: Insuree/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Insuree insuree = db.Insurees.Find(id);
            if (insuree == null)
            {
                return HttpNotFound();
            }
            return View(insuree);
        }

        // POST: Insuree/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,EmailAddress,DateOfBirth,CarYear,CarMake,CarModel,DUI,SpeedingTickets,CoverageType,Quote")] Insuree insuree)
        {
            if (ModelState.IsValid)
            {
                // Recalculate the quote before saving changes
                insuree.Quote = CalculateQuote(insuree);

                db.Entry(insuree).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(insuree);
        }

        // GET: Insuree/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Insuree insuree = db.Insurees.Find(id);
            if (insuree == null)
            {
                return HttpNotFound();
            }
            return View(insuree);
        }

        // POST: Insuree/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Insuree insuree = db.Insurees.Find(id);
            db.Insurees.Remove(insuree);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Admin()
        {
            var insurees = db.Insurees.ToList(); // Fetch all insurees from the database
            return View(insurees);
        }

        public ActionResult AdminLogin(string username, string password)
        {
            if (username == "admin" && password == "admin")
            {
                return RedirectToAction("Admin"); // Redirect to the Admin view on success
            }
            else
            {
                TempData["AdminLoginError"] = "Invalid username or password.";
                return RedirectToAction("Index", "Home"); // Redirect back to home on failure
            }
        }
    }

    
}
