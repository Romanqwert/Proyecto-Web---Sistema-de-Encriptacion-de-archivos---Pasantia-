import { Children } from "react";

interface ButtonProps {
  type?: "primary" | "success" | "danger";
  btnType: "submit" | "button";
  styles?: object;
  onclick?: () => void;
  children: React.ReactNode;
}

const Button: React.FC<ButtonProps> = ({ type, btnType, styles, onclick, children }) => {
  switch (String(type).toLocaleLowerCase()) {
    case "primary":
      return (
        <>
          <button
            type={btnType}
            onClick={() => onclick}
            style={styles}
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
            onClick={() => onclick}
            style={styles}
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
            onClick={() => onclick}
            style={styles}
            className="w-full h-auto bg-[#264E72] text-white text-medium text-center rounded-sm shadow-sm p-2 cursor-pointer hover:-translate-y-1 hover:shadow-sm transition-all"
          >
            {Children.map(children, (child) => child)}
          </button>
        </>
      );
  }
};

export default Button;
