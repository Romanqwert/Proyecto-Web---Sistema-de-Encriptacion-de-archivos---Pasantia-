import Logo from "./Logo.jsx";

interface NavProps {
  showSidebar?: () => void;
  username?: string;
}

const Nav: React.FC<NavProps> = ({ showSidebar, username }) => {
  return (
    <>
      <header className="bg-[#264E72] w-full sticky top-0 shadow">
        <div className="sm:flex justify-center items-center hidden">
          <nav className="w-full px-4 flex justify-between items-center">
            <div>
              <button onClick={showSidebar}>
                <i
                  className="fa-solid fa-grip-lines"
                  style={{
                    color: "white",
                    fontSize: "22px",
                    cursor: "pointer",
                  }}
                ></i>
              </button>
            </div>
            <Logo num={-1} />
            <div className="flex flex-row gap-2">
              <i
                className="fa-solid fa-user"
                style={{ color: "white", fontSize: "20px" }}
              ></i>
              <h2 className="text-xl">
                <strong>{username}</strong>
              </h2>
            </div>
          </nav>
        </div>
        <div className="sm:hidden block">
          <nav className="w-full px-4 flex justify-between items-center">
            <div>
              <button onClick={showSidebar}>
                <i
                  className="fa-solid fa-grip-lines"
                  style={{ color: "white", fontSize: "22px" }}
                ></i>
              </button>
            </div>
            <Logo num={-1} />
          </nav>
        </div>
      </header>
    </>
  );
};
export default Nav;
