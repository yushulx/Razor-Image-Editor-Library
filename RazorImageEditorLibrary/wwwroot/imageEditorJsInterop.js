export function init() {
    return new Promise((resolve, reject) => {
        let script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = '_content/RazorImageEditorLibrary/ddv.js';
        script.onload = async () => {
            let jszip = document.createElement('script');
            jszip.type = 'text/javascript';
            jszip.src = 'https://cdnjs.cloudflare.com/ajax/libs/jszip/3.7.1/jszip.min.js';
            document.head.appendChild(jszip);

            let css = document.createElement('link');
            css.rel = 'stylesheet';
            css.href = '_content/RazorImageEditorLibrary/ddv.css';
            document.head.appendChild(css);
            resolve();
        };
        script.onerror = () => {
            reject();
        };
        document.head.appendChild(script);
    });
}

export async function setLicense(license) {
    if (!Dynamsoft) return;
    try {
        await Dynamsoft.DDV.setConfig({
            license: license,
            engineResourcePath: "_content/RazorImageEditorLibrary/engine",
        });
        Dynamsoft.DDV.setProcessingHandler("imageFilter", new Dynamsoft.DDV.ImageFilter());
        return 0;
    }
    catch (ex) {
        console.error(ex);
        return -1;
    }
}

export async function loadPdfWasm(docManager) {
    if (!Dynamsoft) return;
    try {

        let response = await fetch('_content/RazorImageEditorLibrary/twain.pdf');
        let blob = await response.blob();  
        let doc = docManager.createDocument();
        await doc.loadSource(blob);
        await doc.saveToJpeg(0);
        docManager.deleteDocuments([doc.uid]);
    }
    catch (ex) {
        console.error(ex);
    }
}

export async function createDocumentManager() {
    if (!Dynamsoft) return;

    try {
        let docManager = Dynamsoft.DDV.documentManager;
        docManager.createDocument();
        return docManager;
    }
    catch (ex) {
        console.error(ex);
    }
    return null;
}

export function createEditViewer(docManager, elementId) {
    if (!Dynamsoft) return;

    try {
        let config = {
            type: Dynamsoft.DDV.Elements.Layout,
                flexDirection: "column",
                    className: "ddv-edit-viewer-desktop",
                        children: [
                            {
                                type: Dynamsoft.DDV.Elements.Layout,
                                className: "ddv-edit-viewer-header-desktop",
                                children: [
                                    {
                                        type: Dynamsoft.DDV.Elements.Layout,
                                        children: [
                                            Dynamsoft.DDV.Elements.Zoom,
                                            Dynamsoft.DDV.Elements.FitMode,
                                            Dynamsoft.DDV.Elements.RotateLeft,
                                            Dynamsoft.DDV.Elements.RotateRight,
                                            Dynamsoft.DDV.Elements.Crop,
                                            Dynamsoft.DDV.Elements.Filter,
                                            Dynamsoft.DDV.Elements.Undo,
                                            Dynamsoft.DDV.Elements.Redo,
                                            Dynamsoft.DDV.Elements.DeleteCurrent,
                                            Dynamsoft.DDV.Elements.DeleteAll,
                                        ],
                                    },
                                    {
                                        type: Dynamsoft.DDV.Elements.Layout,
                                        children: [
                                            {
                                                type: Dynamsoft.DDV.Elements.Pagination,
                                                className: "ddv-edit-viewer-pagination-desktop",
                                            },
                                        ],
                                    },
                                ],
                            },
                            Dynamsoft.DDV.Elements.MainView,
                        ],
};


        let editViewer = new Dynamsoft.DDV.EditViewer({
            container: document.getElementById(elementId),
            uiConfig: config,
        });
        editViewer.displayMode = "single"
        openDocument(docManager, editViewer);
        return editViewer;
    }
    catch (ex) {
        console.error(ex);
    }
    return null;

}

export function createBrowseViewer(docManager, elementId, dotNetHelper, cbName) {
    if (!Dynamsoft) return;

    try {
        let browseViewer = new Dynamsoft.DDV.BrowseViewer({
            container: document.getElementById(elementId),
        });
        browseViewer.multiselectMode = true;
        browseViewer.on("currentIndexChanged", (e) => {
            dotNetHelper.invokeMethodAsync(cbName, e.newIndex);
        });
        openDocument(docManager, browseViewer);
        return browseViewer;
    }
    catch (ex) {
        console.error(ex);
    }
    return null;

}

export function openDocument(docManager, viewer) {
    if (!Dynamsoft) return;
    let docs = docManager.getAllDocuments();
    let doc = docs.length == 0 ? docManager.createDocument() : docs[0];
    viewer.openDocument(doc.uid);
}

function saveBlob(blob, fileName) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

function createNumberArray(num) {
    return [...Array(num).keys()];
}

export async function convert(docManager, filename, format, isZip, browseViewer) {
    if (!Dynamsoft || docManager.getAllDocuments().length == 0) return;

    let zip = null;
    if (isZip) {
        zip = new JSZip();
    }

    try {
        let doc = docManager.getAllDocuments()[0];
        let result = null;
        let indices = browseViewer == null ? createNumberArray(doc.pages.length) : browseViewer.getSelectedPageIndices();
        let count = indices.length;

        if (count == 0) return;

        switch (format) {
            case "pdf":
                // https://www.dynamsoft.com/document-viewer/docs/api/interface/idocument/index.html#savetopdf
                const pdfSettings = {
                    author: "Dynamsoft",
                    compression: "pdf/jpeg",
                    pageType: "page/a4",
                    creator: "DDV",
                    creationDate: "D:20230101085959",
                    keyWords: "samplepdf",
                    modifiedDate: "D:20230101090101",
                    producer: "Dynamsoft Document Viewer",
                    subject: "SamplePdf",
                    title: "SamplePdf",
                    version: "1.5",
                    quality: 90,
                }
                result = await doc.saveToPdf(indices, pdfSettings);

                if (zip != null) {
                    zip.file(filename + "." + format, result);
                    zip.generateAsync({ type: "blob" })
                        .then(function (content) {
                            saveBlob(content, filename + ".zip");
                        });
                }
                else {
                    saveBlob(result, filename + "." + format);
                }
                
                break;
            case "png":
                {
                    // https://www.dynamsoft.com/document-viewer/docs/api/interface/idocument/index.html#savetopng
                    

                    if (zip != null) {
                        for (let i = 0; i < count; i++) {
                            result = await doc.saveToPng(indices[i]);
                            zip.file(filename + i + "." + format, result);
                        }
                        
                        zip.generateAsync({ type: "blob" })
                            .then(function (content) {
                                saveBlob(content, filename + ".zip");
                            });
                    }
                    else {
                        for (let i = 0; i < count; i++) {
                            result = await doc.saveToPng(indices[i]);
                            saveBlob(result, filename + i + "." + format);
                        }
                    }
                }

                break;
            case "jpeg":
                {
                    // https://www.dynamsoft.com/document-viewer/docs/api/interface/idocument/index.html#savetojpeg
                    const settings = {
                        quality: 80,
                    };

                    if (zip != null) {
                        for (let i = 0; i < count; i++) {
                            result = await doc.saveToJpeg(indices[i]);
                            zip.file(filename + i + "." + format, result);
                        }

                        zip.generateAsync({ type: "blob" })
                            .then(function (content) {
                                saveBlob(content, filename + ".zip");
                            });
                    }
                    else {
                        for (let i = 0; i < count; i++) {
                            result = await doc.saveToJpeg(indices[i], settings);
                            saveBlob(result, filename + i + "." + format);
                        }
                    }
                }

                break;
            case "tiff":
                // https://www.dynamsoft.com/document-viewer/docs/api/interface/idocument/index.html#savetotiff
                const customTag1 = {
                    id: 700,
                    content: "Created By Dynamsoft",
                    contentIsBase64: false,
                }

                const tiffSettings = {
                    customTag: [customTag1],
                    compression: "tiff/auto",
                }
                result = await doc.saveToTiff(indices, tiffSettings);

                if (zip != null) {
                    zip.file(filename + "." + format, result);
                    zip.generateAsync({ type: "blob" })
                        .then(function (content) {
                            saveBlob(content, filename + ".zip");
                        });
                }
                else {
                    saveBlob(result, filename + "." + format);
                }
                
                break;
        }
    }
    catch (ex) {
        console.error(ex);
    }
} 

export function getFileCount(element) {

    return element.files.length;
}

function endsWith(str, searchString, length) {
    if (length === undefined || length > str.length) {
        length = str.length;
    }
    return str.substring(length - searchString.length, length) === searchString;
}

export function existPDF(element) {
    let files = element.files
    for (let i = 0; i < files.length; i++) {
        if (endsWith(files[i].name, ".pdf")) {
            return true;
        }  
    }

    return false;
}

export function readFileData(element, index) {
    if (!Dynamsoft) return;
    let file = element.files[index];
    return new Promise((resolve, reject) => {

        const reader = new FileReader();
        reader.onload = function (e) {
            const blob = new Blob([e.target.result], { type: file.type });
            resolve(blob);
        };

        reader.onerror = function (err) {
            console.error("Error reading file", err);
            reject();
        };

        reader.readAsArrayBuffer(file);
            
    });    
}

export async function loadPage(docManager, blob, browseViewer, editViewer) {
    if (!Dynamsoft) return;

    try {
        let docs = docManager.getAllDocuments();
        let doc = docs.length == 0 ? docManager.createDocument() : docs[0];
        let index = doc.pages.length;
        let pages = await doc.loadSource(blob);
        
        if (browseViewer) {
            
            let selectedIndices = browseViewer.getSelectedPageIndices();
            for (let i = 0; i < pages.length; i++) {
                selectedIndices.push(i + index);
            }
            browseViewer.selectPages(selectedIndices);
            goToPage(browseViewer, index);

            if (editViewer) {
                goToPage(editViewer, index);
            }
        }
    }
    catch (ex) {
        console.error(ex);
    }
}

export function goToPage(viewer, index) {
    if (viewer != null) {
        viewer.goToPage(index);
    }
}

export function toggleViewer(viewer, show) {
    if (viewer != null) {
        if (show) {
            viewer.show();
        }
        else {
            viewer.hide();
        }
    }
}

export function selectAll(viewer) {
    if (viewer != null) {
        viewer.selectAllPages();
    }
}

export function unselectAll(viewer) {
    if (viewer != null) {
        viewer.selectPages([]);
    }
}

export function removeSelected(docManager, viewer) {
    if (viewer != null) {
        let selectedIndices = viewer.getSelectedPageIndices();
        let docs = docManager.getAllDocuments();
        let doc = docs.length == 0 ? docManager.createDocument() : docs[0];
        doc.deletePages(selectedIndices);
    }
}

export function removeAll(docManager) {
    if (docManager != null) {
        let docs = docManager.getAllDocuments();
        let doc = docs.length == 0 ? docManager.createDocument() : docs[0];
        doc.deleteAllPages();
    }
}

export function readCanvasData(canvas) {
    if (!Dynamsoft) return;

    return new Promise((resolve, reject) => {
        canvas.toBlob(function (blob) {
            resolve(blob);
        });
    });
}