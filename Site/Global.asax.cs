﻿using log4net.Config;
using Sdl.Web.Mvc;
using Sdl.Web.Tridion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Site
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            //routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.IgnoreRoute("cid/{*pathInfo}");
            
            //Tridion page route
            routes.MapRoute(
               "TridionPage",
               "{*PageId}",
               new { controller = "Page", action = "Page" }, 
               new { pageId = @"^(.*)?$" } 
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        protected void Application_Start()
        {
            Configuration.StaticFileManager = new Sdl.Web.DD4T.BinaryFileManager();
            Configuration.SetLocalizations(TridionConfig.PublicationMap);
            var currentVersion = Configuration.CurrentVersion;
            Configuration.Load(Server.MapPath("~"));
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            ViewEngines.Engines.Clear();
            //Register Custom Razor View Engine
            ViewEngines.Engines.Add(new ContextAwareViewEngine());
        }

        protected void Application_BeginRequest()
        {
            //TODO this is an attempt to determin if the request is coming from XPM
            var referrer = HttpContext.Current.Request.UrlReferrer;
            if (referrer != null && referrer.Host == new Uri(Configuration.GetCmsUrl()).Host)
            {
                var cookie = new HttpCookie("cms-edit-mode", "edit");
                Response.Cookies.Add(cookie);
            }
        }
    }
}