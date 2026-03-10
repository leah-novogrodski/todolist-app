import { createContext, useContext, useEffect, useState } from "react";
const getCookie = (name) => {
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) return parts.pop().split(';').shift();
  return null;
};

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);


  useEffect(() => {
     const token = getCookie("token");
    if (token) {
      const payload = parseJwt(token);

      if (payload.exp * 1000 < Date.now()) {
        logout();
      } else {
        setUser(payload);
        scheduleAutoLogout(payload.exp);
      }
    }
  }, []);

  function login(token) {
    const payload = parseJwt(token);

    localStorage.setItem("token", token);
    setUser(payload);
    scheduleAutoLogout(payload.exp);
  }

  function logout() {
    localStorage.removeItem("token");
    setUser(null);
  }

  function scheduleAutoLogout(exp) {
    const delay = exp * 1000 - Date.now();
    setTimeout(logout, delay);
  }

  function parseJwt(token) {
    try {
      return JSON.parse(atob(token.split(".")[1]));
    } catch {
      return null;
    }
  }

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
