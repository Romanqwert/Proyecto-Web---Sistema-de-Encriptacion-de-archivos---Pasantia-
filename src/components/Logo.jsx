import logo from "../assets/logo.png";
import logo2 from "../assets/logo2.png";

const Logo = ({ num }) => {
  switch (Number(num)) {
    case 1:
      return (
        <>
          <img
            src={logo}
            alt="logo-br"
            className=" w-[150px] h-[150px] object-contain object-center"
          />
        </>
      );
    case 2:
      return (
        <>
          <img
            src={logo2}
            alt="logo-br"
            className="w-[450px] object-contain object-center"
          />
        </>
      );
    default:
      return (
        <>
          <img
            src={logo}
            alt="logo-br"
            className=" w-[100px] h-[100px] object-contain object-center"
          />
        </>
      );
  }
};

export default Logo;
