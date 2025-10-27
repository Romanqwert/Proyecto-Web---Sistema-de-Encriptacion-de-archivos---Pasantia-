import Nav from "../components/Navbar";
import Sidebar from "../components/Sidebar";
import { useState } from "react";
import FileUploader from "../components/FileUploader";
import toast from "react-hot-toast";
import { Navigate } from "react-router-dom";
import { decodeToken, IToken } from "../Functions/token";
import { uploadFile } from "../api/files";
import cryptoJs from "crypto-js";

const DecryptPage = () => {
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
  const [key, setKey] = useState<string>("");

  const handleShowSideBar = () => {
    setShowSideBar(!showSideBar);
  };

  const formatSize = (size: number) => {
    if (size < 1024) return `${size} bytes`;
    else if (size < 1024 * 1024) return `${Number(size).toFixed(2)} kb`;
    else if (size < 1024 * 1024 * 1024) return `${Number(size).toFixed(2)} mb`;
  };

  const checkFileSize = (size: number) => {
    if (size < 10485760) {
      toast.error("El archivo no puede ser mayor de 10 MB");
      return;
    }
  };

  const wordArrayToUint8Array = (wordArray: any) => {
    const len = wordArray.sigBytes;
    const words = wordArray.words;
    const u8 = new Uint8Array(len);
    let offset = 0;
    for (let i = 0; i < words.length; i++) {
      let word = words[i];
      u8[offset++] = (word >>> 24) & 0xff;
      if (offset >= len) break;
      u8[offset++] = (word >>> 16) & 0xff;
      if (offset >= len) break;
      u8[offset++] = (word >>> 8) & 0xff;
      if (offset >= len) break;
      u8[offset++] = word & 0xff;
      if (offset >= len) break;
    }
    return u8;
  };

  const handleDecryptDownload = async () => {
    const secretKey = key;

    if (!secretKey) {
      toast.error("Debe ingresar una llave para desencriptar");
      return;
    }

    for (const file of uploadedFiles) {
      try {
        const encryptedBase64 = await file.text();

        const decryptedWords = cryptoJs.AES.decrypt(encryptedBase64, secretKey);

        if (!decryptedWords || decryptedWords.sigBytes <= 0) {
          throw new Error("Decryption produced no data");
        }

        const u8 = wordArrayToUint8Array(decryptedWords);

        const decryptedBlob = new Blob([u8], {
          type: "application/octet-stream",
        });
        const originalName = file.name.replace(".enc", "");

        const link = document.createElement("a");
        link.href = URL.createObjectURL(decryptedBlob);
        link.download = originalName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        try {
          const formData = new FormData();
          formData.append(
            "file",
            new File([decryptedBlob], originalName, {
              type: "application/octet-stream",
            })
          );
          await uploadFile(formData);
          toast.success("Archivo desencriptado subido correctamente");
        } catch (err) {
          console.error(err);
          toast.error("Error al subir el archivo desencriptado");
        }
      } catch (err) {
        console.error(err);
        toast.error(
          "Error al desencriptar el archivo (llave incorrecta o archivo corrupto)"
        );
      }
    }
  };

  return (
    <>
      <Nav showSidebar={handleShowSideBar} />
      <div className="flex justify-between h-auto">
        {showSideBar && <Sidebar />}
        <main className="w-full" onClick={() => setShowSideBar(false)}>
          <section className="h-full w-full">
            <div className="grid mt-12 place-items-center">
              <div className="w-full h-full px-4 sm:max-w-3xl space-y-2">
                <div>
                  <h2 className="text-2xl font-bold">
                    Desencriptar Archivo('s)
                  </h2>
                  <p>
                    Asegurese de tener la llave valida y la cual se utilizo para
                    encriptar el archivo
                  </p>
                  <br />
                </div>
                <div>
                  <FileUploader
                    filterFiles=".enc"
                    setUploadedFiles={setUploadedFiles}
                  />
                </div>
                {uploadedFiles.length > 0 && (
                  <div className="grid gap-4">
                    <div>
                      <h4 className="text-gray-800 text-sm mb-2">
                        Ingrese la llave para desencriptar:
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
                      onClick={handleDecryptDownload}
                      className="w-full h-auto bg-[#264E72] text-white text-medium text-center rounded-sm shadow-sm p-2 cursor-pointer hover:-translate-y-1 hover:shadow-sm transition-all"
                    >
                      desencriptar
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

export default DecryptPage;
