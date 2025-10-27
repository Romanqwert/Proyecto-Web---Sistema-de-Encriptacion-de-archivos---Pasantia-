import { data } from "react-router-dom";
import { api, Response } from "./api";

const token = sessionStorage.getItem("user_token");

export const listFiles = async (id: number): Promise<Response> => {
  return await api.get(`/api/Archivos/list/${id}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });
};

export const downloadFile = async (id: number): Promise<Response> => {
  return await api.get(`/api/Archivos/${id}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });
};

export const uploadFile = async (file: FormData): Promise<Response> => {
  return await api.post("/api/Archivos/upload", file, {
    headers: {
      Authorization: `Bearer: ${token}`,
      "Content-Type": "multipart/form-data",
    },
  });
};
