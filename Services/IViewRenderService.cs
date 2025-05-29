namespace QLSV.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderViewAsync<TModel>(string viewName, TModel model, bool isMainView = false);

    }
}
