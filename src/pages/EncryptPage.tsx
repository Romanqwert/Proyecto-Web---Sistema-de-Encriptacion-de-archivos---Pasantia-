import Nav from "../components/Navbar";
import Sidebar from "../components/Sidebar";
import { useState } from "react";
import FileUploader from "../components/FileUploader";
import Button from "../components/Button";
import { Navigate } from "react-router-dom";
import { Toaster, toast } from "react-hot-toast";

type Token = {
  Name: string;
  id: number;
} | null;

const EncryptPage = () => {
  const storedToken = sessionStorage.getItem("user_token");
  const token = storedToken ? JSON.parse(storedToken) : null;
  console.log(token);
  if (!token) {
    return <Navigate to="/login" replace />;
  }
  toast.success(`Bienvenido ${token?.Name}!`);

  const [showSideBar, setShowSideBar] = useState<boolean>(false);
  const [uploadedFiles, setUploadedFiles] = useState<File[]>([]);

  const handleShowSideBar = () => {
    setShowSideBar(!showSideBar);
  };

  const formatSize = (size: number) => {
    if (size < 1024) return `${size} bytes`;
    else if (size < 1024 * 1024) return `${Number(size).toFixed(2)} kb`;
    else if (size < 1024 * 1024 * 1024) return `${Number(size).toFixed(2)} mb`;
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
                  <h2 className="text-2xl font-bold">Encriptar Archivo('s)</h2>
                  <p>
                    Asegurese de ingresar los archivos correctos y con sus
                    respectivas condiciones para ser encriptados de forma
                    exitosa.
                  </p>
                  <br />
                </div>
                <div>
                  <FileUploader setUploadedFiles={setUploadedFiles} />
                </div>
                {uploadedFiles.length > 0 && (
                  <div>
                    <Button btnType={"submit"} type={"primary"}>encriptar</Button>
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
