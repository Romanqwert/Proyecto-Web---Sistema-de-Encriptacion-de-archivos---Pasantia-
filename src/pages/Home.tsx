import Nav from "../components/Navbar";
import { Toaster, toast } from "react-hot-toast";
import Sidebar from "../components/Sidebar";
import { useState } from "react";
import { Navigate } from "react-router-dom";
import { decodeToken, IToken } from "../Functions/token";
import { listFiles } from "../api/files";
import { isAxiosError } from "axios";
import { useEffect } from "react";

type FileRecord = {
  NombreArchivo?: string;
  TipoMime?: string;
  TamanoBytes?: number;
  FechaSubida?: string;
} | null;

const HomePage = () => {
  const storedToken = sessionStorage.getItem("user_token");
  const token: IToken | null = storedToken ? decodeToken(storedToken) : null;
  const [data, setData] = useState<FileRecord[]>([]);

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

  const [showSideBar, setShowSideBar] = useState(false);
  const handleShowSideBar = () => {
    setShowSideBar(!showSideBar);
  };

  const getFiles = async (): Promise<boolean> => {
    try {
      const response = await listFiles(id);
      setData(response.data || []);
      return true;
    } catch (error: unknown) {
      if (isAxiosError(error)) {
        const serverMessage =
          (error.response &&
            (error.response.data?.message || error.response.data)) ||
          error.message;
        toast.error(serverMessage || "Error al obtener archivos");
      } else {
        toast.error("Error inesperado");
      }
      return false;
    }
  };

  const formatSize = (size?: number) => {
    if (!size && size !== 0) return "-";
    if (size < 1024) return `${size} bytes`;
    if (size < 1024 * 1024) return `${(size / 1024).toFixed(2)} KB`;
    return `${(size / (1024 * 1024)).toFixed(2)} MB`;
  };

  useEffect(() => {
    const fetchFiles = async () => {
      const ok = await getFiles();
      if (ok) toast.success(`Bienvenido ${username}!`);
    };

    fetchFiles();
  }, [id]);

  return (
    <>
      <Toaster position="bottom-right" reverseOrder={false} />
      <Nav showSidebar={handleShowSideBar} />
      <div className="flex justify-between h-auto">
        {showSideBar && <Sidebar />}
        <main className="w-full" onClick={() => setShowSideBar(false)}>
          <section className="grid place-content-center gap-10 mt-5 w-full h-full px-5">
            {data.length > 0 && (
              <div className="space-y-1">
                <h2 className="text-3xl font-bold">Historial de Archivos</h2>
                <p className="ml-1">Encriptados o Desencriptados.</p>
              </div>
            )}
            <div className="grid place-content-center w-full gap-3 sm:max-w-7xl px-4">
              {data.length > 0 ? (
                <ul className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {data.map((file: FileRecord | null, index: any) => {
                    return (
                      <li key={index}>
                        <div className="flex flex-row items-center px-3 py-2 gap-4 w-full bg-gray-200 shadow-sm rounded-sm">
                          <div>
                            <i
                              className="fa-solid fa-file"
                              style={{ color: "gray", fontSize: "43px" }}
                            ></i>
                          </div>
                          <div className="space-y-1.5">
                            <h2 className="text-xl font-bold">
                              {file?.NombreArchivo}
                            </h2>
                            <hr />
                            <p className="text-sm sm:text-base">
                              Tamaño del archivo:{" "}
                              <strong> {formatSize(file?.TamanoBytes)}</strong>
                            </p>
                            <p className="text-sm sm:text-base">
                              Subido en el <strong> {file?.FechaSubida}</strong>
                            </p>
                          </div>
                        </div>
                      </li>
                    );
                  })}
                </ul>
              ) : (
                <div className="h-dvh flex justify-center items-center flex-col gap-5">
                  <i
                    className="fa-solid fa-face-frown-open"
                    style={{ fontSize: "40px", color: "gray" }}
                  ></i>
                  <p className="text-center text-gray-500 font-medium">
                    No hay historial de archivos
                  </p>
                </div>
              )}
            </div>
          </section>
        </main>
      </div>
    </>
  );
};

export default HomePage;
