import { useState, useRef } from "react";
import toast, { Toaster } from "react-hot-toast";

const FileUploader = ({ setUploadedFiles }) => {
  const fileInputRef = useRef(null);
  const [files, setFiles] = useState([]);
  const [isDragging, setIsDragging] = useState(false);

  const handleDrop = (e) => {
    e.preventDefault();

    const targetFiles = Array.from(e.dataTransfer.files);
    targetFiles.forEach((file) => {
      console.log(file);
      if (file.size > 10 * 1024 * 1024)
        toast.error("El archivo no puede ser de mas de 10 MB");
    });

    const uploadedFiles = targetFiles.filter((f) => f.size < 10 * 1024 * 1024);
    if (!uploadedFiles) {
      toast.error(
        "No se han procesado los archivos exitosamente, intente nuevamente"
      );
      return;
    }

    setFiles(uploadedFiles);
    setUploadedFiles(uploadedFiles);
  };

  const handleDragOver = (e) => {
    e.preventDefault();
  };

  const handleDragEnter = () => setIsDragging(true);
  const handleDragLeave = () => setIsDragging(false);

  const fileClick = () => fileInputRef.current.click();
  const handleFiles = (e) => {
    const targetFiles = Array.from(e.target.files);
    targetFiles.forEach((file) => {
      console.log(file);
      if (file.size > 10 * 1024 * 1024)
        toast.error("El archivo no puede ser de mas de 10 MB");
    });

    const uploadedFiles = targetFiles.filter((f) => f.size < 10 * 1024 * 1024);
    if (!uploadedFiles) {
      toast.error(
        "No se han procesado los archivos exitosamente, intente nuevamente"
      );
      return;
    }

    console.log(targetFiles, uploadedFiles);

    setFiles(uploadedFiles);
    setUploadedFiles(uploadedFiles);
  };

  return (
    <>
      <Toaster position="bottom-right" reverseOrder={false} />
      <div
        onClick={fileClick}
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragEnter={handleDragEnter}
        onDragLeave={handleDragLeave}
        className={
          isDragging
            ? "w-full h-full min-h-50 p-4 border-2 border-gray-500 border-dashed grid place-content-center place-items-center gap-4 bg-black/25 cursor-pointer hover:border-blue-800 hover:border-3 transition-all"
            : "w-full h-full min-h-50 p-4 border-2 border-gray-500 border-dashed grid place-content-center place-items-center gap-4 cursor-pointer hover:border-blue-800 hover:border-3 transition-all"
        }
      >
        <i
          className={files.length === 0 ? "fa-solid fa-plus-minus" : "hidden"}
          style={{ color: "gray", fontSize: "33px" }}
        ></i>
        <p className="text-gray-600 font-medium">
          {files.length === 0
            ? "Haga click o Arrastre y suelte los archivos a subir..."
            : `${files.length} Archivos seleccionados`}
        </p>
        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept=".json,application/json,.xml,text/xml,.config"
          className="hidden"
          name="files"
          onChange={handleFiles}
        />
      </div>
    </>
  );
};
export default FileUploader;
