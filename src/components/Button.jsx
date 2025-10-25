import { Children } from "react";

//  primary, danger, success
const Button = ({ type, btnType, styles, onClick, children }) => {
  switch (String(type).toLocaleLowerCase()) {
    case "primary":
      return (
        <>
          <button
            type={btnType}
            onClick={() => onClick}
            style={styles}
            data-aos="fade-in"
            className="w-full h-auto bg-[#264E72] text-white text-medium text-center rounded-sm shadow-sm p-2 cursor-pointer hover:-translate-y-1 hover:shadow-sm transition-all"
          >
            {Children.map(children, (child) => child)}
          </button>
        </>
      );
    case "danger":
      return (
        <>
          <button
            onClick={() => onClick}
            style={styles}
            data-aos="fade-in"
            className="w-full h-auto bg-[#264E72] text-white text-medium text-center rounded-sm shadow-sm p-2 cursor-pointer hover:-translate-y-1 hover:shadow-sm transition-all"
          >
            {Children.map(children, (child) => child)}
          </button>
        </>
      );
    default:
      return (
        <>
          <button
            type={btnType}
            onClick={() => onClick}
            style={styles}
            data-aos="fade-in"
            className="w-full h-auto bg-[#264E72] text-white text-medium text-center rounded-sm shadow-sm p-2 cursor-pointer hover:-translate-y-1 hover:shadow-sm transition-all"
          >
            {Children.map(children, (child) => child)}
          </button>
        </>
      );
  }
};

export default Button;
