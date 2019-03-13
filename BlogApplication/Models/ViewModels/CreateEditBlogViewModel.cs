using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlogApplication.Models.ViewModels
{
    public class CreateEditBlogViewModel
    {
        [Required(ErrorMessage = "The Title field is required")]
        public string BlogTitle { get; set; }
        [AllowHtml]
        public string Body { get; set; }
        [Required(ErrorMessage = "The Published field is required")]
        public bool Published { get; set; }
        [Required(ErrorMessage = "The Date field is required")]
        public DateTime DateCreated { get; set; }
        public HttpPostedFileBase Picture { get; set; }
    }
}