using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog_jcf.Models;
using PagedList;
using PagedList.Mvc;
using Microsoft.AspNet.Identity;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;

namespace Blog_jcf.Controllers
{
    [RequireHttps]
    public class BlogPostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

       

        // GET: BlogPosts

        public static class ImageUploadValidator
        {
            public static bool IsWebFriendlyImage(HttpPostedFileBase file)
            {
                // check for actual object
                if (file == null)
                    return false;

                //check size - file must be less than 2 MB and greater than 1 KB
                if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 1024)
                    return false;

                try
                {
                    using (var img = Image.FromStream(file.InputStream))
                    {
                        return ImageFormat.Jpeg.Equals(img.RawFormat) ||
                               ImageFormat.Png.Equals(img.RawFormat) ||
                               ImageFormat.Gif.Equals(img.RawFormat);
                    }
                }


                catch
                {
                    return false;
                }
            }
        }
       
        public ActionResult Index(int? page, string query)
        {
            int pageSize = 3; // the number of posts you want to display per page
            int pageNumber = (page ?? 1);
            ViewBag.Query = query;
            var qposts = db.Posts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                qposts = qposts.Where(p => p.Title.Contains(query) || p.Body.Contains(query) || p.Comments.Any(c => c.Body.Contains(query) || c.Author.DisplayName.Contains(query)));
            }
       
            var posts = qposts.OrderByDescending(p => p.Created).ToPagedList(pageNumber, pageSize); // code added to replace this code: "var posts = db.Posts.ToList();"
            return View(posts); //return View(db.Posts.ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Admin()
        {
            var posts = db.Posts.ToList();
            return View(posts); //return View(db.Posts.ToList());
        }

        // GET: BlogPosts/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(string slug)
        {
            if (String.IsNullOrWhiteSpace(slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.FirstOrDefault(p => p.Slug == slug);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "Id,Title,Slug,Body,MediaURL,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                //restricting the valid file formats to images only
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/img_uploads/"), fileName));
                    blogPost.MediaURL = "~/img_uploads/" + fileName;
                }


                var Slug = StringUtilities.URLFriendly(blogPost.Title);
                if (String.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid title.");
                    return View(blogPost);
                }
                if (db.Posts.Any(p => p.Slug == Slug))
                {
                    ModelState.AddModelError("Title", "The title must be unique.");
                    return View(blogPost);
                }

                blogPost.Slug = Slug;
                blogPost.Created = System.DateTimeOffset.Now;
                db.Posts.Add(blogPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,Title,Body,MediaURL,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                //restricting the valid file formats to images only
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/img_uploads/"), fileName));
                    blogPost.MediaURL = "!/img_uploads/" + fileName;
                }


                blogPost.Updated = System.DateTimeOffset.Now;
                db.Posts.Attach(blogPost);
                //db.Entry(blogPost).Property("Title").IsModified = true;
                db.Entry(blogPost).Property("Body").IsModified = true;
                //db.Entry(blogPost).Property("MediaURL").IsModified = true;
                //db.Entry(blogPost).Property("Slug").IsModified = true;
                db.Entry(blogPost).Property("Updated").IsModified = true;

                //db.Entry(blogPost).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            BlogPost blogPost = db.Posts.Find(id);
            db.Posts.Remove(blogPost);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: BlogPosts/CreateComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize]
        public ActionResult CreateComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.AuthorId = User.Identity.GetUserId();
                comment.Created = System.DateTimeOffset.Now;
                //comment.Updated = System.DateTimeOffset.Now;
                db.Comments.Add(comment);
                db.SaveChanges();

            }
            var blog = db.Posts.Find(comment.PostId);
            return RedirectToAction("Details", "BlogPosts", new { slug = blog.Slug }); //need to send in a SLUG
        }

        // GET: BlogPosts/EditComments
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult EditComments(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: BlogPosts/EditComments
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult EditComments(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.AuthorId = User.Identity.GetUserId();
                //comment.Created = System.DateTimeOffset.Now;
                comment.Updated = System.DateTimeOffset.Now;

                db.Comments.Attach(comment);
                db.Entry(comment).Property("Body").IsModified = true;
                db.Entry(comment).Property("AuthorId").IsModified = true;
                db.Entry(comment).Property("Updated").IsModified = true;

                db.SaveChanges();

            }

            var blog = db.Posts.Find(comment.PostId);
            return RedirectToAction("Details", "BlogPosts", new { slug = blog.Slug }); //need to send in a SLUG
        }

        // GET: BlogPosts/DeleteComments
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult DeleteComments(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult DeleteComments(int id)
        {
            Comment comment = db.Comments.Find(id);
            var slug = comment.Post.Slug;
            db.Comments.Remove(comment);
            db.SaveChanges();


            return RedirectToAction("Details", "BlogPosts", new { slug = slug }); //need to send in a SLUG

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

