using CalculatorMvc5.Models;
using CalculatorMvc5.Repository;
using CalculatorMvc5.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CalculatorMvc5.Controllers
{
    
    public class HomeController : Controller
    {
        // Member fields
        private CalculationsHistoryRepository m_rep;
        
        // Constructors
        public HomeController()
        {
            m_rep = new CalculationsHistoryRepository();
        }

        public HomeController(CalculationsHistoryRepository rep)
        {
            m_rep = rep ?? new CalculationsHistoryRepository();
        }

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetCalculationsHistory()
        {
            //Thread.Sleep(10000);
            string ip = Helper.GetIP();
            List<CalculationsHistory> calcList = m_rep.GetCalculationsHistoryByIPandCurrentDate(ip);
            return Json(calcList, JsonRequestBehavior.AllowGet);
        }

        // GET: Home/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Home/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Home/Create
        [HttpPost]
        public ActionResult Create(string expression)
        {
            bool insertedStatus = false;
            try
            {
                // TODO: Add insert logic here
                string ip = Helper.GetIP();
                insertedStatus = m_rep.InsertCalculatedExpression(ip, expression);
                if(insertedStatus)
                {
                    CalculationsHistory lastInsertedRow = m_rep.GetLastInsertedRow<CalculationsHistory>("CalculationsHistory", "Id");
                    return Json(new
                    {
                        lastInsertedRow
                    });
                }

                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Home"),
                    isRedirect = true
                });
            }
            catch(Exception ex)
            {
                LogFile.WriteToLog(ex.Message +"\n"+ex.StackTrace);
                return View();
            }
        }

        // GET: Home/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Home/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }        

        // POST: Home/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            bool status = false;
            string message = "";

            try
            {
                // TODO: Add delete logic here
                status = m_rep.DeleteCalculatedExpression(id.ToString());
                if (status)
                    message = "Successfully Deleted!";
                
                return Json(new
                {
                    Data = new { status = status, message = message }
                });
            }
            catch
            {
                return HttpNotFound();
            }
        }
    }
}
