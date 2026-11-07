// Lets the WebGL build save a file by starting a browser download
mergeInto(LibraryManager.library, {
    DownloadFile: function (fileNamePtr, dataPtr, length) {
        var fileName = UTF8ToString(fileNamePtr);
        // Copy the bytes out of the Unity heap, which can move or be reused after this call
        var bytes = HEAPU8.slice(dataPtr, dataPtr + length);
        var url = URL.createObjectURL(new Blob([bytes], { type: "image/png" }));

        var link = document.createElement("a");
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        setTimeout(function () { URL.revokeObjectURL(url); }, 0);
    }
});
