import Logo from "../components/Logo";
import Button from "../components/Button";
import "./styles.css";
import { useEffect } from "react";
import { Link } from "react-router-dom";
import { toast, Toaster } from "react-hot-toast";

function RegisterPage() {
  useEffect(() => {
    document.body.classList.add("body");
  });

  const handleRegister = (data) => {
    const user = data.get("username");
    const email = data.get("email");
    const password = data.get("password");
    const passwordConfirm = data.get("passwordConfirm");

    console.log(user, email, password, passwordConfirm);

    if (!user || !email || !password || !passwordConfirm) {
      toast.error("Llene el formulario");
      return;
    }

    // todo: add api request to register the user to the db
    // then once success redirect to login.
  };

  return (
    <>
      <Toaster position="bottom-right" reverseOrder={false} />
      <header className="bg-[#264E72] w-full flex justify-center items-center absolute">
        <nav className="w-full sm:max-w-7xl p-4 flex justify-center items-center">
          <Logo num={1} />
        </nav>
      </header>
      <main className="w-full h-full px-4">
        <div className="flex justify-center items-center relative mb-2 top-[150px]">
          <section className="w-full h-auto sm:max-w-3xl bg-white rounded-2xl shadow-sm px-10">
            <div className="p-4">
              <h2 className="text-[#264E72] font-black text-3xl text-center mb-6">
                REGISTRO DE USUARIO
              </h2>
              <form
                action={handleRegister}
                className="flex flex-col gap-5"
                method="post"
              >
                <div className="flex sm:flex-row flex-col sm:justify-evenly items-center gap-2">
                  <div className="w-full">
                    <label
                      htmlFor="username"
                      className="font-medium text-[#555555]"
                    >
                      Nombre completo:
                    </label>
                  </div>
                  <div className="w-full">
                    <input
                      id="username"
                      name="username"
                      type="text"
                      className="bg-[#EDEDEDEB] rounded-sm p-2 w-full outline-0 max-w-md focus:outline-2 focus:outline-blue-500 transition-all"
                    />
                  </div>
                </div>
                <div className="flex sm:flex-row flex-col sm:justify-evenly items-center gap-2">
                  <div className="w-full">
                    <label
                      htmlFor="email"
                      className="font-medium text-[#555555]"
                    >
                      Email:
                    </label>
                  </div>
                  <div className="w-full">
                    <input
                      id="email"
                      name="email"
                      type="email"
                      className="bg-[#EDEDEDEB] rounded-sm p-2 w-full outline-0 max-w-md focus:outline-2 focus:outline-blue-500 transition-all"
                    />
                  </div>
                </div>
                <div className="flex sm:flex-row flex-col sm:justify-evenly items-center gap-2">
                  <div className="w-full">
                    <label
                      htmlFor="password"
                      className="font-medium text-[#555555]"
                    >
                      Contraseña:
                    </label>
                  </div>
                  <div className="w-full">
                    <input
                      id="password"
                      name="password"
                      type="password"
                      className="bg-[#EDEDEDEB] rounded-sm p-2 w-full outline-0 max-w-md focus:outline-2 focus:outline-blue-500 transition-all"
                    />
                  </div>
                </div>
                <div className="flex sm:flex-row flex-col sm:justify-evenly items-center gap-2">
                  <div className="w-full">
                    <label
                      htmlFor="password_confirmation"
                      className="font-medium text-[#555555]"
                    >
                      Confirmar contraseña:
                    </label>
                  </div>
                  <div className="w-full">
                    <input
                      id="password_confirmation"
                      name="passwordConfirm"
                      type="password"
                      className="bg-[#EDEDEDEB] rounded-sm p-2 w-full outline-0 max-w-md focus:outline-2 focus:outline-blue-500 transition-all"
                    />
                  </div>
                </div>
                <div className="flex sm:flex-row sm:flex-nowrap flex-wrap justify-center items-center gap-4 w-full">
                  <Button type={"primary"}>Acceder</Button>
                  <div className="flex flex-row items-center justify-center gap-2 w-full">
                    <p className="text-[#555555] text-sm">
                      Ya tienes un usuario?
                    </p>
                    <Link to="/login">
                      <span className="text-blue-500 underline text-sm">
                        Ingresa aqui
                      </span>
                    </Link>
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
export default RegisterPage;
