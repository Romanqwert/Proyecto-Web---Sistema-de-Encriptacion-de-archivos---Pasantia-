import Nav from "../components/Navbar";
import Logo from "../components/Logo";
import "./styles.css";
import { useEffect, useState } from "react";
import Button from "../components/Button";
import { Link } from "react-router-dom";
import { toast, Toaster } from "react-hot-toast";

function LoginPage() {
  const [showPassword, setShowPassword] = useState(false);

  useEffect(() => {
    document.body.classList.add("body");
  });

  const handleLogin = (data) => {
    const username = data.get("username");
    const password = data.get("password");

    if (!username || !password) {
      toast.error("Llene el formulario para iniciar sesion.");
      return;
    }

    // todo: call api to check if user is registered
    // if true: redirect to home page with token created if user exists
    // else: show toast error telling the user that the credentials does not exist
    // or that they dont match.
  };

  return (
    <>
      <Toaster position="bottom-right" reverseOrder={false} />
      <main className="w-full h-full px-4">
        <div className="flex justify-center items-center mb-2 h-[90dvh] top-[150px]">
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
                              class={
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
