import { Link } from "react-router-dom";
import Button from "../components/Button";

const NotFoundPage = () => {
  return (
    <>
      <main className="w-full px-4 ">
        <section className="w-full h-full text-center sm:text-left">
          <div className="h-dvh grid place-content-center place-items-center gap-5">
            <div>
              <i
                className="fa-solid fa-face-frown"
                style={{ color: "#264E72", fontSize: "120px" }}
              ></i>
            </div>
            <div className="space-y-5">
              <h2 className="text-2xl text-[#264E72] font-bold">
                Oops, parece que fuiste un poco lejos
              </h2>
              <p>Intenta regresar al inicio.</p>
              <Link to="/">
                <Button type={"primary"} btnType={"button"}>
                  Click aqui
                </Button>
              </Link>
            </div>
          </div>
        </section>
      </main>
      <footer className="fixed bottom-0 w-full p-4">
        <div className="grid place-content-center place-items-center">
          <p>© Copyright - 2025</p>
        </div>
      </footer>
    </>
  );
};

export default NotFoundPage;
