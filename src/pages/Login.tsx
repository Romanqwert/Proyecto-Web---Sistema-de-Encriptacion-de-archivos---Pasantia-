import Logo from "../components/Logo";
import "./styles.css";
import { useEffect, useState } from "react";
import Button from "../components/Button";
import { Link, Navigate } from "react-router-dom";
import { toast, Toaster } from "react-hot-toast";
import { login } from "../api/auth";
import { AxiosError } from "axios";

interface Response {
  statusCode?: number;
  message?: string;
  token?: string;
}

function LoginPage() {
  const [showPassword, setShowPassword] = useState<boolean>(false);

  useEffect(() => {
    document.body.classList.add("body");
  });

  const handleLogin = async (data: FormData) => {
    const username = data.get("username") ?? null;
    const password = data.get("password") ?? null;

    if (!username || !password) {
      toast.error("Llene el formulario para iniciar sesion.");
      return;
    }

    try {
      const response: Response = await login({
        correoElectronico: username.toString(),
        passwordHash: password.toString(),
      });

      const token = response.token;

      if (!token) {
        toast.error("Inicio de sesion invalido, intente nuevamente");
        return;
      }

      sessionStorage.setItem("user_token", token);
      window.location.href = "http://localhost:3000/";
    } catch (error: unknown) {
      if (error instanceof AxiosError) {
        toast.error(error.response?.data);
        return;
      }
    }
  };

  return (
    <>
      <Toaster position="bottom-right" reverseOrder={false} />
      <main className="w-full h-full px-4">
        <div className="flex justify-center items-center mb-2 h-dvh top-[150px]">
          <section className="w-full h-auto sm:max-w-3xl bg-white rounded-2xl shadow-sm ">
            <div className="p-4 pt-0 pb-7">
              <div className="flex justify-center w-full">
                <Logo num={2} />
              </div>

              <form method="post" action={handleLogin}>
                <div className="flex justify-evenly flex-wrap md:flex-nowrap sm:justify-items-end gap-1">
                  <div className="sm:w-[50%] flex flex-col gap-5 w-full">
                    <div>
                      <div className="w-full">
                        <input
                          id="username"
                          name="username"
                          type="text"
                          placeholder="Usuario"
                          className="bg-[#EDEDEDEB] rounded-sm p-4 text-[#555555] w-full outline-0 max-w-md focus:outline-2 focus:outline-blue-500 transition-all"
                        />
                      </div>
                    </div>

                    <div>
                      <div className="w-full">
                        <div className="flex flex-col items-end gap-2">
                          <button
                            type="button"
                            onClick={() => setShowPassword(!showPassword)}
                          >
                            <i
                              className={
                                !showPassword
                                  ? "fa-solid fa-eye-slash"
                                  : "fa-solid fa-eye"
                              }
                              style={{
                                color: "gray",
                                fontSize: "20px",
                                cursor: "pointer",
                              }}
                            ></i>
                          </button>
                          <input
                            id="password"
                            name="password"
                            type={showPassword ? "text" : "password"}
                            placeholder="Contraseña"
                            className="bg-[#EDEDEDEB] rounded-sm p-4 text-[#555555] w-full outline-0 max-w-md focus:outline-2 focus:outline-blue-500 transition-all"
                          />
                          <span className="text-[12px]">
                            <a href="#">Olvide mi contraseña</a>
                          </span>
                        </div>
                      </div>
                    </div>

                    <Button btnType={"submit"} type={"primary"}>
                      Acceder
                    </Button>
                  </div>
                  <div className="flex items-end">
                    <div className="flex justify-center items-end gap-2 w-full">
                      <p className="text-[#555555] text-sm">
                        No tienes un usuario?
                      </p>
                      <Link to="/register">
                        <span className="text-blue-500 underline text-sm">
                          Ingresa aqui
                        </span>
                      </Link>
                    </div>
                  </div>
                </div>
              </form>
            </div>
          </section>
        </div>
      </main>
    </>
  );
}

export default LoginPage;
