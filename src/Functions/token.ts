import { jwtDecode } from "jwt-decode";

export type IToken = {
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": string;
  exp: number;
  iss: string;
  aud: string;
} | null;

export const decodeToken = (token: string) => {
  return jwtDecode<IToken>(token);
};
