import { api, Response } from "./api";

type LoginUserData = {
  correoElectronico?: string;
  passwordHash?: string;
};

type RegisterUserData = {
  nombreUsuario?: string;
  correoElectronico?: string;
  passwordHash?: string;
  fecahRegistro?: string;
};

export const login = async (userData: LoginUserData): Promise<Response> => {
  return (await api.post("/login", userData)).data;
};

export const register = async (
  userData: RegisterUserData
): Promise<Response> => {
  return (await api.post("/register", userData)).data;
};

export const profile = async (): Promise<Response> => {
  return await api.get("/perfil");
};
