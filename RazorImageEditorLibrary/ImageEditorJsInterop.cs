using Microsoft.JSInterop;

namespace RazorImageEditorLibrary
{
    public class ImageEditorJsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public ImageEditorJsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/RazorImageEditorLibrary/imageEditorJsInterop.js").AsTask());
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

        public async Task LoadJS()
        {
            var module = await moduleTask.Value;
            await module.InvokeAsync<object>("init");
        }

        public async Task<int> SetLicense(string license)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<int>("setLicense", license);
        }

        public async Task<DocumentManager> CreateDocumentManager()
        {
            var module = await moduleTask.Value;
            IJSObjectReference jsObjectReference = await module.InvokeAsync<IJSObjectReference>("createDocumentManager");
            DocumentManager docManager = new DocumentManager(module, jsObjectReference);
            return docManager;
        }
    }
}