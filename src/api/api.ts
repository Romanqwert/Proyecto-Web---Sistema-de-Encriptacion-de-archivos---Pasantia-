import axios, { AxiosInstance } from "axios";
import { API_BASE_URL } from "./config";

const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
});

export default api;
