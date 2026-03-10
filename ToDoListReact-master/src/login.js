import { useState } from "react";
import "./login.css";
import axios from "./service"; // ייבוא הקובץ שהגדרנו למעלה
import { useNavigate } from "react-router-dom";

export default function Login({ onLogin }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();

  // בתוך פונקציית handleLogin
  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      // קריאה לפונקציה שהגדרנו בשירות
      const data = await axios.login(username, password);
      if (data && data.token) {
        // שמירת הטוקן בעוגייה (חשוב!)
        document.cookie = `token=${data.token}; path=/; max-age=3600; SameSite=Strict`;
        alert("Logged in successfully!");
        navigate("/");
      }
    } catch (err) {
      console.error("Login Error:", err);
      alert("Login failed.");
    }
  };

  return (
    <form onSubmit={handleLogin}>
      <h2>Login</h2>

      <input
        placeholder="Username"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
      />

      <input
        type="password"
        placeholder="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />

      <button type="submit">Login</button>
      <span className="register-link">
        Don't have an account? <a href="/register">Register</a>
      </span>
    </form>
  );
}
