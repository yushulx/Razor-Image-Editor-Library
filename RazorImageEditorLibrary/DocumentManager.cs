﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorImageEditorLibrary
{
    public class DocumentManager
    {
        private IJSObjectReference _module;
        private IJSObjectReference _jsObjectReference;
        private IJSObjectReference? browseViewer = null, editViewer = null;
        private DotNetObjectReference<DocumentManager> objRef;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the DocumentNormalizer class.
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

        public async Task CreateBrowseViewer(string elementId)
        {
            browseViewer = await _module.InvokeAsync<IJSObjectReference>("createBrowseViewer", _jsObjectReference, elementId, objRef, "OnPageIndexChanged");
        }

        public async Task CreateEditViewer(string elementId)
        {
            editViewer = await _module.InvokeAsync<IJSObjectReference>("createEditViewer", _jsObjectReference, elementId);
        }

        public async Task Convert(string filename, string format, bool isZip)
        {
            await _module.InvokeVoidAsync("convert", _jsObjectReference, filename, format, isZip, browseViewer);
        }

        public async Task LoadPdfWasm()
        {
            await _module.InvokeVoidAsync("loadPdfWasm", _jsObjectReference);
        }

        [JSInvokable]
        public async Task OnPageIndexChanged(int index)
        {
            if (editViewer != null)
            {
                await _module.InvokeVoidAsync("goToPage", editViewer, index);
            }
        }

        public async Task ShowEditor()
        {
            await _module.InvokeVoidAsync("toggleViewer", editViewer, true);
        }

        public async Task HideEditor()
        {
            await _module.InvokeVoidAsync("toggleViewer", editViewer, false);
        }

        public async Task SelectAll()
        {
            await _module.InvokeVoidAsync("selectAll", browseViewer);
        }

        public async Task UnselectAll()
        {
            await _module.InvokeVoidAsync("unselectAll", browseViewer);
        }

        public async Task RemoveSelected()
        {
            await _module.InvokeVoidAsync("removeSelected", _jsObjectReference, browseViewer);
        }

        public async Task RemoveAll()
        {
            await _module.InvokeVoidAsync("removeAll", _jsObjectReference);
        }

        public async Task LoadCanvas(IJSObjectReference canvas)
        {
            IJSObjectReference blob = await _module.InvokeAsync<IJSObjectReference>("readCanvasData", canvas);
            await _module.InvokeVoidAsync("loadPage", _jsObjectReference, blob, browseViewer, editViewer);
        }
    }
}
