import { Link } from "react-router-dom";

interface SidebarProps {
  showSideBar?: () => void;
}

const SideBar: React.FC<SidebarProps> = ({ showSideBar }) => {
  return (
    <>
      <aside
        className="bg-[#264E72] text-white w-[300px] h-full fixed top-25% left-0"
        data-aos="fade-right"
      >
        <div className="grid place-items-start gap-5 p-3">
          <Link className="w-full p-2" to={"/encriptar"}>
            <button className="flex justify-between items-center text-start font-medium gap-2 cursor-pointer">
              <i className="fa-solid fa-lock" style={{ fontSize: "20px" }}></i>
              Encriptar archivo
            </button>
          </Link>
          <Link className="w-full p-2" to={"/desencriptar"}>
            <button className="flex justify-between items-center text-start font-medium gap-2 cursor-pointer">
              <i
                className="fa-solid fa-lock-open"
                style={{ fontSize: "20px" }}
              ></i>
              Desencriptar archivo
            </button>
          </Link>
          <Link className="w-full p-2" to={"/"}>
            <button className="flex justify-between items-center text-start font-medium gap-2 cursor-pointer">
              <i className="fa-solid fa-book" style={{ fontSize: "20px" }}></i>
              Historial
            </button>
          </Link>
        </div>
      </aside>
    </>
  );
};

export default SideBar;
