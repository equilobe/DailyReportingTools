using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class AvatarController : BaseMvcController
    {
       public IDataService DataService { get; set; }

        // GET: Avatar
        public ActionResult Image(string id)
        {
            var image = DataService.GetUserImageByKey(id);
            return File(image, "image/png");
        }
    }
}