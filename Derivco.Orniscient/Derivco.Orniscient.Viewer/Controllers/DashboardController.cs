﻿using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Viewer.Clients;
using Derivco.Orniscient.Viewer.Models.Dashboard;
using Orleans;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Derivco.Orniscient.Viewer.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public async Task<ActionResult> Index(int id = 0)
        {
            try
            {
                this.ViewBag.AllowMethodsInvocation = AllowMethodsInvocation();

                await Task.Run(async () => await GrainClientInitializer.InitializeIfRequired(Server.MapPath("~/DevTestClientConfiguration.xml")));
                return View();
            }
            catch (Exception ex)
            {
                return View("InitError");
            }
        }

        public async Task<ActionResult> GetDashboardInfo()
        {
            var dashboardCollectorGrain = GrainClient.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

            var types = await dashboardCollectorGrain.GetGrainTypes();
            var dashboardInfo = new DashboardInfo
            {
                Silos = await dashboardCollectorGrain.GetSilos(),
                AvailableTypes = types
            };

            return Json(dashboardInfo, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFilters(GetFiltersRequest filtersRequest)
        {
            if (filtersRequest?.Types == null)
                return null;

            var filterGrain = GrainClient.GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
            var filters = await filterGrain.GetGroupedFilterValues(filtersRequest.Types);
            return Json(filters, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> GetGrainInfo(GetGrainInfoRequest grainInfoRequest)
        {
            var typeFilterGrain = GrainClient.GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
            var filters = await typeFilterGrain.GetFilters(grainInfoRequest.GrainType, grainInfoRequest.GrainId);
            return Json(filters);
        }

        [HttpPost]
        public async Task SetSummaryViewLimit(int summaryViewLimit, int sessionId = 0)
        {
            var dashboardInstanceGrain = GrainClient.GrainFactory.GetGrain<IDashboardInstanceGrain>(sessionId);
            await dashboardInstanceGrain.SetSummaryViewLimit(summaryViewLimit);
        }

        [HttpPost]
        public async Task<ActionResult> GetMethods(string type)
        {
            var methodGrain = GrainClient.GrainFactory.GetGrain<ITypeMethodsGrain>(type);
            var methods = await methodGrain.GetAvailableMethods();
            return Json(methods, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> InvokeGrainMethod(string type, string id, string methodId, string parametersJson)
        {
            if (AllowMethodsInvocation())
            {
                var methodGrain = GrainClient.GrainFactory.GetGrain<ITypeMethodsGrain>(type);
                try
                {
                    var methodReturnData = await methodGrain.InvokeGrainMethod(id, methodId, parametersJson);
                    return Json(methodReturnData, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Error: " + ex.Message);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Error: This method cannot be invoked");
        }

        private bool AllowMethodsInvocation()
        {
            bool allowMethodsInvocation;
            if (!bool.TryParse(ConfigurationManager.AppSettings["AllowMethodsInvocation"], out allowMethodsInvocation))
            {
                allowMethodsInvocation = true;
            }

            return allowMethodsInvocation;
        }
    }
}