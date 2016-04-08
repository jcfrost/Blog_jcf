using Blog_jcf.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Blog_jcf.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        //public ActionResult Abc()
        //{
        //    ViewBag.Pqr = "This is the New Page!";
        //    return View();
        //}
        //public ActionResult MyInfo1()
        //{
        //    ViewBag.MyInfoMessage = "This is about me!";
        //    return View();
        //}

      
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact([Bind(Include = "Id,Name,Email,Message,Phone,MessageSent")] Contact contact)
        {
            contact.MessageSent = DateTime.Now;

            var svc = new EmailService();
            var msg = new IdentityMessage();
            msg.Subject = "Contact From Portfolio";
            msg.Body = "Email from: " + contact.Name + " (" + contact.Email + ") <br/>" + "Message: <br/>" + contact.Message;
            await svc.SendAsync(msg);

            return View(contact);
        }
    }
}