import { BrowserRouter, Routes, Route } from "react-router-dom";
import LoginPage from "./pages/Login";
import RegisterPage from "./pages/Register";
import HomePage from "./pages/Home";
import EncryptPage from "./pages/EncryptPage";
import DecryptPage from "./pages/DecryptPage";

function App() {
  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/" element={<HomePage />} />
          <Route path="/encriptar" element={<EncryptPage />} />
          <Route path="/desencriptar" element={<DecryptPage />} />
        </Routes>
      </BrowserRouter>
    </>
  );
}

export default App;
