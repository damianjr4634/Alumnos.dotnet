// Descarga de archivos generados en el servidor (Excel/PDF de EsbaListView).
// Recibe el contenido en base64 y dispara la descarga en el navegador sin
// recargar el circuito Blazor.
window.esbaDescargarArchivo = (nombre, contenidoBase64, tipoMime) => {
    const binario = atob(contenidoBase64);
    const bytes = new Uint8Array(binario.length);
    for (let i = 0; i < binario.length; i++) {
        bytes[i] = binario.charCodeAt(i);
    }

    const blob = new Blob([bytes], { type: tipoMime });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = nombre;
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);
    URL.revokeObjectURL(url);
};
