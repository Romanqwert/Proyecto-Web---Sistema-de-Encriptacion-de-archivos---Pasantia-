import Nav from "../components/Navbar";
import Sidebar from "../components/Sidebar";
import { useState } from "react";
import FileUploader from "../components/FileUploader";
import { Navigate } from "react-router-dom";
import { Toaster, toast } from "react-hot-toast";
import { IToken, decodeToken } from "../Functions/token";
import { uploadFile } from "../api/files";
import cryptoJs from "crypto-js";

const EncryptPage = () => {
  const storedToken = sessionStorage.getItem("user_token");
  const token: IToken | null = storedToken ? decodeToken(storedToken) : null;

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  const username =
    token["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
  const email =
    token["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"];
  const id = Number(
    token[
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
    ]
  );

  const [showSideBar, setShowSideBar] = useState<boolean>(false);
  const [uploadedFiles, setUploadedFiles] = useState<File[]>([]);
  const [key, setKey] = useState<string>();

  const handleShowSideBar = () => {
    setShowSideBar(!showSideBar);
  };

  const formatSize = (size: number) => {
    if (size < 1024) return `${size} bytes`;
    else if (size < 1024 * 1024) return `${Number(size).toFixed(2)} kb`;
    else if (size < 1024 * 1024 * 1024) return `${Number(size).toFixed(2)} mb`;
  };

  const handleEncryptAndDownload = async () => {
    const secretKey = key;

    if (!secretKey) {
      toast.error("Debe ingresar una llave para encriptar");
      return;
    }

    const uint8ArrayToWordArray = (u8arr: Uint8Array) => {
      const len = u8arr.length;
      const words: number[] = [];
      for (let i = 0; i < len; i += 4) {
        words.push(
          (u8arr[i] << 24) |
            ((u8arr[i + 1] || 0) << 16) |
            ((u8arr[i + 2] || 0) << 8) |
            (u8arr[i + 3] || 0)
        );
      }
      return cryptoJs.lib.WordArray.create(words, len);
    };

    for (const file of uploadedFiles) {
      const arrayBuffer = await file.arrayBuffer();
      const u8 = new Uint8Array(arrayBuffer);
      const wordArray = uint8ArrayToWordArray(u8);

      const encryptedBase64 = cryptoJs.AES.encrypt(
        wordArray,
        secretKey
      ).toString();

      const encryptedBlob = new Blob([encryptedBase64], { type: "text/plain" });
      const encryptedFile = new File([encryptedBlob], `${file.name}.enc`, {
        type: "text/plain",
      });

      const link = document.createElement("a");
      link.href = URL.createObjectURL(encryptedBlob);
      link.download = encryptedFile.name;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      try {
        const formData = new FormData();
        formData.append("file", encryptedFile);
        const resp = await uploadFile(formData);
        toast.success("Archivo encriptado subido correctamente");
      } catch (err) {
        console.error(err);
        toast.error("Error al subir el archivo encriptado");
      }
    }
  };

  return (
    <>
      <Toaster position="bottom-right" reverseOrder={false} />
      <Nav showSidebar={handleShowSideBar} />
      <div className="flex justify-between h-auto">
        {showSideBar && <Sidebar />}
        <main className="w-full" onClick={() => setShowSideBar(false)}>
          <section className="h-full w-full">
            <div className="grid mt-12 place-items-center">
              <div className="w-full h-full px-4 sm:max-w-3xl space-y-2">
                <div>
                  <h2 className="text-2xl font-bold">Encriptar Archivo('s)</h2>
                  <p>
                    Asegurese de ingresar los archivos correctos y con sus
                    respectivas condiciones para ser encriptados de forma
                    exitosa.
                  </p>
                  <br />
                </div>
                <div>
                  <FileUploader
                    filterFiles=".json,application/json,.xml,text/xml,.config"
                    setUploadedFiles={setUploadedFiles}
                  />
                </div>
                {uploadedFiles.length > 0 && (
                  <div className="grid gap-4">
                    <div>
                      <h4 className="text-gray-800 text-sm mb-2">
                        Ingrese la llave para encriptar:
                      </h4>
                      <input
                        value={key}
                        onChange={(e) => setKey(e.target.value)}
                        type="text"
                        name="key"
                        id="key"
                        className="w-full p-2 shadow-sm rounded-sm bg-gray-200 text-gray-500 outline-0 focus:outline-blue-500 focus:outline-2"
                      />
                    </div>
                    <button
                      onClick={handleEncryptAndDownload}
                      className="w-full h-auto bg-[#264E72] text-white text-medium text-center rounded-sm shadow-sm p-2 cursor-pointer hover:-translate-y-1 hover:shadow-sm transition-all"
                    >
                      encriptar
                    </button>
                  </div>
                )}
                {uploadedFiles.length > 0 && (
                  <div>
                    <ul className="grid gap-5">
                      {uploadedFiles.map((file, index) => {
                        console.log(file);
                        return file ? (
                          <li key={index}>
                            <div className="flex flex-row items-center px-3 py-2 gap-4 w-full bg-gray-100  rounded-sm">
                              <div>
                                <i
                                  className="fa-solid fa-file"
                                  style={{ color: "gray", fontSize: "43px" }}
                                ></i>
                              </div>
                              <div className="space-y-1.5">
                                <h2 className="text-xl font-bold">
                                  {file.name}
                                </h2>
                                <hr />
                                <p className="text-sm sm:text-base">
                                  Tamaño del archivo:
                                  <strong> {formatSize(file.size)}</strong>
                                </p>
                                <p className="text-sm sm:text-base">
                                  Ultima vez modificado:
                                  <strong>
                                    {file?.lastModified?.toString()}
                                  </strong>
                                </p>
                                <p className="text-sm sm:text-base">
                                  Tipo de archivo:
                                  <strong> {file.type}</strong>
                                </p>
                              </div>
                            </div>
                          </li>
                        ) : (
                          ""
                        );
                      })}
                    </ul>
                  </div>
                )}
              </div>
            </div>
          </section>
        </main>
      </div>
    </>
  );
};

export default EncryptPage;
