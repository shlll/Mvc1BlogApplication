using BlogApplication.Models;
using BlogApplication.Models.Blog;
using BlogApplication.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlogApplication.Controllers
{
    public class BlogPostController : Controller
    {
        private ApplicationDbContext DbContext = new ApplicationDbContext();
        // GET: BlogPost
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var blog = DbContext.Blogs.Where(p => p.UserId == userId)
                .Select(p => new IndexBlogViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    Published = p.Published,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                }).ToList();
            return View(blog);
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(CreateEditBlogViewModel data)
        {
            return SaveBlogPost(null, data);
        }
        public ActionResult SaveBlogPost(int? id, CreateEditBlogViewModel data)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var userId = User.Identity.GetUserId();
            if (DbContext.Blogs.Any(p =>
            p.Title == data.BlogTitle &&
            (!id.HasValue || p.Id != id.Value)))
            {
                ModelState.AddModelError(nameof(CreateEditBlogViewModel.BlogTitle),
                    "Blog's title should be special");
                return View();
            }
            string fileExtension;
            //Validating file upload
            if (data.Picture != null)
            {
                fileExtension = Path.GetExtension(data.Picture.FileName);
                
                if (!Constants.AllowedFileExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "File extension is not allowed.");
                    return View();
                }
            }
            BlogPost blogPost;
            if (!id.HasValue)
            {
                blogPost = new BlogPost();
                blogPost.UserId = userId;
                DbContext.Blogs.Add(blogPost);
            }
            else
            {
                blogPost = DbContext.Blogs.FirstOrDefault(p => p.Id == id);
                if (blogPost == null)
                {
                    return RedirectToAction(nameof(BlogPostController.Index));
                }
            }
            blogPost.Title = data.BlogTitle;
            blogPost.Body = data.Body;
            blogPost.Published = data.Published;
            data.DateCreated = DateTime.Now;
            if (data.Picture != null)
            {
                if (!Directory.Exists(Constants.MappedUploadFolder))
                {
                    Directory.CreateDirectory(Constants.MappedUploadFolder);
                }

                var fileName = data.Picture.FileName;
                var fullPathWithName = Constants.MappedUploadFolder + fileName;

                data.Picture.SaveAs(fullPathWithName);

                blogPost.PictureUrl = Constants.UploadFolder + fileName;
            }
            DbContext.SaveChanges();
            return RedirectToAction(nameof(BlogPostController.Index));
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(BlogPostController.Index));
            }
            var userId = User.Identity.GetUserId();
            var blogModel = DbContext.Blogs.FirstOrDefault(
                p => p.Id == id && p.UserId == userId);
            if (blogModel == null)
            {
                return RedirectToAction(nameof(BlogPostController.Index));
            }
            var model = new CreateEditBlogViewModel();
            model.BlogTitle = blogModel.Title;
            model.Body = blogModel.Body;
            model.Published = blogModel.Published;
            model.DateCreated = DateTime.Now;
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(int id, CreateEditBlogViewModel data)
        {
            return SaveBlogPost(id, data);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(BlogPostController.Index));
            }
            var userId = User.Identity.GetUserId();
            var blogModel = DbContext.Blogs.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (blogModel != null)
            {
                DbContext.Blogs.Remove(blogModel);
                DbContext.SaveChanges();
            }
            return RedirectToAction(nameof(BlogPostController.Index));
        }
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(BlogPostController.Index));
            }
            var userId = User.Identity.GetUserId();
            var blogModel = DbContext.Blogs.FirstOrDefault(p =>
            p.Id == id.Value &&
            p.UserId == userId);
            if(blogModel == null)
            {
                return RedirectToAction(nameof(BlogPostController.Index));
            }
            var model = new DetailsBlogViewModel();
            model.Title = blogModel.Title;
            model.Body = blogModel.Body;
            model.Published = blogModel.Published;
            model.DateCreated = DateTime.Now;
            model.DateUpdated = DateTime.Now;
            model.PictureUrl = blogModel.PictureUrl;
            return View(model);
        }

    }
}