import Nav from "../components/Navbar";
import Sidebar from "../components/Sidebar";
import { useState } from "react";
import FileUploader from "../components/FileUploader";
import toast from "react-hot-toast";
import Button from "../components/Button";

const DecryptPage = () => {
  onload((e) => {
    if (!token) {
      throw redirect(
        `/login?errorMessage=${"usuario invalido, por favor authenticate."}`
      );
    }
    toast.success(`Bienvenido ${token.Name}!`);
  });

  const [showSideBar, setShowSideBar] = useState(false);
  const [uploadedFiles, setUploadedFiles] = useState([]);

  const handleShowSideBar = () => {
    setShowSideBar(!showSideBar);
  };

  const formatSize = (size) => {
    if (size < 1024) return `${size} bytes`;
    else if (size < 1024 * 1024) return `${Number(size).toFixed(2)} kb`;
    else if (size < 1024 * 1024 * 1024) return `${Number(size).toFixed(2)} mb`;
  };

  const checkFileSize = (size) => {
    if (size < 10485760) {
      toast.error("El archivo no puede ser mayor de 10 MB");
      return;
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
                  <FileUploader setUploadedFiles={setUploadedFiles} />
                </div>
                {uploadedFiles.length > 0 && (
                  <div>
                    <Button btnType={"primary"}>desencriptar</Button>
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
                                    {file.lastModifiedDate.toString()}
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
