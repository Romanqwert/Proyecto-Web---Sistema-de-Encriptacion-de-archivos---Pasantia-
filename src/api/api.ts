import axios, { AxiosInstance } from "axios";
import { API_BASE_URL } from "./config";

export interface Response {
  statusCode?: number;
  message?: string;
  data?: any;
}

export const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
    Authorization: sessionStorage.getItem("user_token"),
  },
});

