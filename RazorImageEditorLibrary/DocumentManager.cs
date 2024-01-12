using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorImageEditorLibrary
{
    /// <summary>
    /// Represents a document manager.
    /// </summary>
    public class DocumentManager
    {
        // Fields to hold JavaScript object references.
        private IJSObjectReference _module;
        private IJSObjectReference _jsObjectReference;
        private IJSObjectReference? browseViewer = null, editViewer = null;
        private DotNetObjectReference<DocumentManager> objRef;
        private bool _disposed = false;
        public bool IsPDFReady = false;
        /// <summary>
        /// Initializes a new instance of the DocumentManager class.
        /// </summary>
        /// <param name="module">A reference to the JavaScript module.</param>
        /// <param name="normalizer">A reference to the JavaScript object for document normalization.</param>
        public DocumentManager(IJSObjectReference module, IJSObjectReference normalizer)
        {
            _module = module;
            _jsObjectReference = normalizer;
            objRef = DotNetObjectReference.Create(this);
        }

        /// <summary>
        /// Release unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed == false)
            {
                objRef.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Destructor for the DocumentManager class.
        /// </summary>
        ~DocumentManager()
        {
            if (_disposed == false)
                Dispose();
        }

        /// <summary>
        /// Loads a file into the document manager.
        /// </summary>
        /// <param name="inputFile">A reference to the input file.</param>
        public async Task LoadFile(ElementReference inputFile)
        {
            int count = await _module.InvokeAsync<int>("getFileCount", inputFile);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    IJSObjectReference blob = await _module.InvokeAsync<IJSObjectReference>("readFileData", inputFile, i);

                    await _module.InvokeVoidAsync("loadPage", _jsObjectReference, blob, browseViewer, editViewer);
                }
            }
        }

        /// <summary>
        /// Checks whether input files contain PDF.
        /// </summary>
        public async Task<bool> ExistPDF(ElementReference inputFile)
        {
            return await _module.InvokeAsync<bool>("existPDF", inputFile);
        }

        /// <summary>
        /// Creates a browse viewer.
        /// </summary>
        /// <param name="elementId">The ID of the element to create the viewer in.</param>
        public async Task CreateBrowseViewer(string elementId)
        {
            browseViewer = await _module.InvokeAsync<IJSObjectReference>("createBrowseViewer", _jsObjectReference, elementId, objRef, "OnPageIndexChanged");
        }

        /// <summary>
        /// Creates an edit viewer.
        /// </summary>
        /// <param name="elementId">The ID of the element to create the viewer in.</param>
        public async Task CreateEditViewer(string elementId)
        {
            editViewer = await _module.InvokeAsync<IJSObjectReference>("createEditViewer", _jsObjectReference, elementId);
        }

        /// <summary>
        /// Converts the document to the specified format.
        /// </summary>
        /// <param name="filename">The filename of the document.</param>
        /// <param name="format">The format to convert to.</param>
        /// <param name="isZip">A value indicating whether the output should be zipped.</param>
        public async Task Convert(string filename, string format, bool isZip)
        {
            await _module.InvokeVoidAsync("convert", _jsObjectReference, filename, format, isZip, browseViewer);
        }

        /// <summary>
        /// Loads the PDF reader module into the browser.
        /// </summary>
        public async Task LoadPdfWasm()
        {
            await _module.InvokeVoidAsync("loadPdfWasm", _jsObjectReference);
            IsPDFReady = true;
        }

        /// <summary>
        /// Invoked when the page index is changed in the browse viewer.
        /// </summary>
        [JSInvokable]
        public async Task OnPageIndexChanged(int index)
        {
            if (editViewer != null)
            {
                await _module.InvokeVoidAsync("goToPage", editViewer, index);
            }
        }

        /// <summary>
        /// Shows the editor.
        /// </summary>
        public async Task ShowEditor()
        {
            await _module.InvokeVoidAsync("toggleViewer", editViewer, true);
        }

        /// <summary>
        /// Hides the editor.
        /// </summary>
        public async Task HideEditor()
        {
            await _module.InvokeVoidAsync("toggleViewer", editViewer, false);
        }

        /// <summary>
        /// Selects all files in the document manager.
        /// </summary>
        public async Task SelectAll()
        {
            await _module.InvokeVoidAsync("selectAll", browseViewer);
        }

        /// <summary>
        /// Unselects all files in the document manager.
        /// </summary>
        public async Task UnselectAll()
        {
            await _module.InvokeVoidAsync("unselectAll", browseViewer);
        }

        /// <summary>
        /// Removes the selected files from the document manager.
        /// </summary>
        public async Task RemoveSelected()
        {
            await _module.InvokeVoidAsync("removeSelected", _jsObjectReference, browseViewer);
        }

        /// <summary>
        /// Removes all files from the document manager.
        /// </summary>
        public async Task RemoveAll()
        {
            await _module.InvokeVoidAsync("removeAll", _jsObjectReference);
        }

        /// <summary>
        /// Loads a canvas into the document manager.
        /// </summary>
        /// <param name="canvas">A reference to the HTML canvas.</param>
        public async Task LoadCanvas(IJSObjectReference canvas)
        {
            IJSObjectReference blob = await _module.InvokeAsync<IJSObjectReference>("readCanvasData", canvas);
            await _module.InvokeVoidAsync("loadPage", _jsObjectReference, blob, browseViewer, editViewer);
        }
    }
}
