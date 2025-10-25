import Nav from "../components/Navbar";
import { Toaster, toast } from "react-hot-toast";
import Sidebar from "../components/Sidebar";
import { useState } from "react";
import { Navigate } from "react-router-dom";

const HomePage = () => {
  const token = JSON.parse(sessionStorage.getItem("user_token")) ?? null;
  console.log(token);
  if (!token) {
    return <Navigate to="/login" replace />;
  }
  toast.success(`Bienvenido ${token?.Name}!`);

  const [showSideBar, setShowSideBar] = useState(false);
  const handleShowSideBar = () => {
    setShowSideBar(!showSideBar);
  };

  const date = new Date();
  const data = [
    {
      fileName: "config.json",
      fileType: ".json",
      filePath: "/",
      fileSize: 1024,
      dateUploaded: date.toString(),
    },
    {
      fileName: "config.config",
      fileType: ".config",
      filePath: "/",
      fileSize: 1024,
      dateUploaded: date.toString(),
    },
    {
      fileName: "connection.xml",
      fileType: ".xml",
      filePath: "/",
      fileSize: 1024,
      dateUploaded: date.toString(),
    },
    {
      fileName: "connection.xml",
      fileType: ".xml",
      filePath: "/",
      fileSize: 1024,
      dateUploaded: date.toString(),
    },
    {
      fileName: "connection.xml",
      fileType: ".xml",
      filePath: "/",
      fileSize: 1024,
      dateUploaded: date.toString(),
    },
    {
      fileName: "connection.xml",
      fileType: ".xml",
      filePath: "/",
      fileSize: 1024,
      dateUploaded: date.toString(),
    },
  ];

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
                  {data.map((file, index) => {
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
                              {file.fileName}
                            </h2>
                            <hr />
                            <p className="text-sm sm:text-base">
                              Tamaño del archivo:{" "}
                              <strong> {file.fileSize}kb</strong>
                            </p>
                            <p className="text-sm sm:text-base">
                              Subido en el <strong> {file.dateUploaded}</strong>
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
                    class="fa-solid fa-face-frown-open"
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
