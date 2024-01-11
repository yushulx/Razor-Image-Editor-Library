using Microsoft.JSInterop;

namespace RazorImageEditorLibrary
{
    public class ImageEditorJsInterop : IAsyncDisposable
    {
        // Holds a task that resolves to a JavaScript module reference.
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        /// <summary>
        /// Initializes a new instance of the ImageEditorJsInterop class.
        /// </summary>
        /// <param name="jsRuntime">The JS runtime to use for invoking JavaScript functions.</param>
        public ImageEditorJsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/RazorImageEditorLibrary/imageEditorJsInterop.js").AsTask());
        }

        /// <summary>
        /// Releases unmanaged resources asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

        /// <summary>
        /// Loads and initializes the JavaScript module.
        /// </summary>
        public async Task LoadJS()
        {
            var module = await moduleTask.Value;
            await module.InvokeAsync<object>("init");
        }


        /// <summary>
        /// Sets the license key for the Dynamsoft Document Viewer SDK.
        /// </summary>
        /// <param name="license">The license key.</param>
        public async Task<int> SetLicense(string license)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<int>("setLicense", license);
        }

        /// <summary>
        /// Creates a new DocumentManager instance.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is a new DocumentManager instance.</returns>
        public async Task<DocumentManager> CreateDocumentManager()
        {
            var module = await moduleTask.Value;
            IJSObjectReference jsObjectReference = await module.InvokeAsync<IJSObjectReference>("createDocumentManager");
            DocumentManager docManager = new DocumentManager(module, jsObjectReference);
            return docManager;
        }
    }
}