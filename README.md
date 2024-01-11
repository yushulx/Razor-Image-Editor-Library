# Razor-Image-Editor-Library
A Razor Class Library built using the [Dynamsoft Document Viewer SDK](https://www.npmjs.com/package/dynamsoft-document-viewer), which is designed to offer a range of viewers for configuring and executing various document processing workflows, such as image editing and format conversion.

## Demo Video
[https://github.com/yushulx/Razor-Image-Editor-Library/assets/2202306/ac99fef4-d171-429a-88e4-ac4a2f1b23fb](https://github.com/yushulx/Razor-Image-Editor-Library/assets/2202306/ac99fef4-d171-429a-88e4-ac4a2f1b23fb)

## Online Demo
[ https://yushulx.me/Razor-Image-Editor-Library/]( https://yushulx.me/Razor-Image-Editor-Library/)

## Prerequisites
- [Dynamsoft Document Viewer License](https://www.dynamsoft.com/customer/license/trialLicense?product=ddv)

## Quick Usage
- Import and initialize the Razor Image Editor Library.
    
    ```csharp
    @using RazorImageEditorLibrary
    
    @code {
        private ImageEditorJsInterop? imageEditorJsInterop;
        
        protected override async Task OnInitializedAsync()
        {
            imageEditorJsInterop = new ImageEditorJsInterop(JSRuntime);
            await imageEditorJsInterop.LoadJS();
        }
    }
    ```

- Set the license key.
    
    ```csharp
    await imageEditorJsInterop.SetLicense(licenseKey);
    ```

- Createa a document manager instance.
    
    ```csharp
    DocumentManager documentManager = await imageEditorJsInterop.CreateDocumentManager();
    ```

- Load PDF reader wasm module if you want to view PDF files.
    
    ```csharp
    await documentManager.LoadPdfWasm();
    ```

- Create browse viewer and edit viewer by binding HTML div element IDs.
    
    ```csharp
    <div id="browse-viewer"></div>
    <div id="edit-viewer"></div>
    
    @code {
        await documentManager.CreateBrowseViewer("browse-viewer");
        await documentManager.CreateEditViewer("edit-viewer");
    }
    ```

- Load an input file or a canvas to the browse viewer.
    
    ```csharp
    await documentManager.LoadFile(inputFile);
    await documentManager.LoadCanvas(canvas);
    ```
- Convert selected images to a specified format.
    
    ```csharp
    string selectedValue = FileFormat.PDF; // or FileFormat.PNG, FileFormat.JPG, FileFormat.TIFF
    await documentManager.Convert("convert", selectedValue, isZip);
    ```

## API

### FileFormat Class
Represents the file format of the output file.

### DocumentManager Class
- `Task LoadPdfWasm()`: Loads the PDF reader wasm module.
- `Task CreateEditViewer(string elementId)`: Creates an edit viewer instance.
- `Task CreateBrowseViewer(string elementId)`: Creates a browse viewer instance.
- `Task LoadFile(ElementReference inputFile)`: Loads an input file to the browse viewer.
- `Task LoadCanvas(IJSObjectReference canvas)`: Loads a canvas to the browse viewer. 
- `Task Convert(string filename, string format, bool isZip)`: Converts selected images to a specified format.
- `Task SelectAll()`: Selects all files in the document manager.
- `Task UnselectAll()`: Unselects all files in the document manager.
- `Task RemoveSelected()`: Removes selected files from the document manager.
- `Task RemoveAll()`: Removes all files from the document manager.

### ImageEditorJsInterop Class 
- `Task LoadJS()`: Loads the Dynamsoft Document Viewer JavaScript library.
- `Task SetLicense(string license)`: Sets the license key.
- `Task<DocumentManager> CreateDocumentManager()`: Creates a Document Manager instance.

## Example
- [Blazor Image Editor/Converter](https://github.com/yushulx/Razor-Image-Editor-Library/tree/main/example)

    ![blazor image editor/converter](https://camo.githubusercontent.com/5b3bf150cba405a81c50f61d7b8da6612fc27276e82b6dab116b156b56836004/68747470733a2f2f7777772e64796e616d736f66742e636f6d2f636f6465706f6f6c2f696d672f323032342f30312f626c617a6f722d696d6167652d656469746f722d636f6e7665727465722e706e67)

## Build
```bash
cd RazorImageEditorLibrary
dotnet build --configuration Release
```