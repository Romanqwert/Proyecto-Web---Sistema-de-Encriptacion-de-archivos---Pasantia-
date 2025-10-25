import { AxiosResponse } from "axios";
import api from "./api";

type LoginUserData = {
  username?: string;
  password?: string;
};

type RegisterUserData = {
  username?: string;
  password?: string;
  email?: string;
};

interface Response {
  statusCode?: number;
  message?: string;
  token?: string;
};

export const login = async (
  userData: LoginUserData
): Promise<Response> => {
  try {
    return (await api.post("/login", userData)).data;
  } catch (error: unknown) {
    if (error instanceof Error) {
      console.error(error.message);
      return {
        message: `Error invalido: ${error.message}, intente otra vez o mas tarde.`,
      };
    } else {
      console.error(error);
      return { message: `Error invalido, intente otra vez o mas tarde.` };
    }
  }
};

export const register = async (
  userData: RegisterUserData
): Promise<Response> => {
  try {
    return (await api.post("/register", userData)).data;
  } catch (error: unknown) {
    if (error instanceof Error) {
      console.error(error.message);
      return {
        message: `Error invalido: ${error.message}, intente otra vez o mas tarde.`,
      };
    } else {
      console.error(error);
      return { message: `Error invalido, intente otra vez o mas tarde.` };
    }
  }
};
export const profile = async (
  token: string
): Promise<Response> => {
  try {
    return await api.get("/perfil", {
      headers: {
        Authorization: `Bearer: ${token}`,
      },
    });
  } catch (error: unknown) {
    if (error instanceof Error) {
      console.error(error.message);
      return {
        message: `Error invalido: ${error.message}, intente otra vez o mas tarde.`,
      };
    } else {
      console.error(error);
      return { message: `Error invalido, intente otra vez o mas tarde.` };
    }
  }
};
