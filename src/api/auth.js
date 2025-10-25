import api from "./api";

export const login = async (userData) => await api.post("/login", userData);

export const register = async (userData) => await api.post("/register", userData);

export const profile = async (token) =>
  await api.get("/perfil", {
    headers: {
      Authorization: `Bearer: ${token}`,
    },
  });
