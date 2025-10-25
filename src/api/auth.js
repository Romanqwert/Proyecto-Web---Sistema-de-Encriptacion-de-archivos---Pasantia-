import api from "./api";

export const login = async (userData) => {
  try {
    await api.post("/login", userData);
  } catch (error) {
    console.error(error.message);
    return `Error invalido: ${error.message}, intente otra vez o mas tarde.`;
  }
};

export const register = async (userData) => {
  try {
    await api.post("/register", userData);
  } catch (error) {
    console.error(error.message);
    return `Error invalido: ${error.message}, intente otra vez o mas tarde.`;
  }
};
export const profile = async (token) => {
  try {
    await api.get("/perfil", {
      headers: {
        Authorization: `Bearer: ${token}`,
      },
    });
  } catch (error) {
    console.error(error.message);
    return `Error invalido: ${error.message}, intente otra vez o mas tarde.`;
  }
};
