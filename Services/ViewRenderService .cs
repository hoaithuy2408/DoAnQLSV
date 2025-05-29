using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;

namespace QLSV.Services
{
    public class ViewRenderService : IViewRenderService
    {
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRazorViewEngine _viewEngine;
        private readonly IHttpContextAccessor _contextAccessor;

        public ViewRenderService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IHttpContextAccessor contextAccessor)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _contextAccessor = contextAccessor;
        }

        public async Task<string> RenderViewAsync<TModel>(string viewName, TModel model, bool isMainView = false)
        {
            var actionContext = new ActionContext(
                _contextAccessor.HttpContext,
                _contextAccessor.HttpContext.GetRouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            );

            var viewResult = _viewEngine.FindView(actionContext, viewName, isMainView);

            if (!viewResult.Success)
                throw new InvalidOperationException($"Không tìm thấy view: {viewName}");

            var viewDictionary = new ViewDataDictionary<TModel>(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
            {
                Model = model
            };

            // ✅ Truyền flag export PDF vào ViewData
            viewDictionary["IsExportPdf"] = true;

            using var sw = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(_contextAccessor.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }

}
