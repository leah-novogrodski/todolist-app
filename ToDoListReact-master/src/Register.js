import { useState } from "react";
import service from './service'; // שיניתי את השם מ-axios ל-service כדי למנוע בלבול
import './login.css';
import { useNavigate } from "react-router-dom";

export default function Register() {
  const [name, setName] = useState("");
  const [password, setPassword] = useState("");
  const [id, setId] = useState("");
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    try {
      // המרה של ה-ID למספר כדי שהשרת לא יזרוק שגיאת JSON
      const numericId = Number(id);
      
      if (isNaN(numericId)) {
        alert("Please enter a valid numeric ID");
        return;
      }

      // קריאה לשירות הרישום
      const data = await service.register(name, password, numericId);

      alert("User created ✔ You can now login.");

      // בדיקה אם השרת החזיר טוקן כבר ברישום (תלוי במימוש שלך בשרת)
      if (data && data.token) {
        document.cookie = `token=${data.token}; path=/; max-age=3600; SameSite=Strict`;
        navigate("/"); // כניסה ישירה למשימות
      } else {
        // אם אין טוקן, שולחים אותו להתחבר בצורה מסודרת
        navigate("/login");
      }
      
    } catch (err) {
      console.error("Registration error:", err);
      alert("Registration failed. User might already exist or server is down.");
    }
  };

  return (
    <div className="login-container"> {/* עטיפה לעיצוב אם קיימת ב-CSS */}
      <form onSubmit={handleRegister}>
        <h2>Register</h2>

        <input
          placeholder="Username"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />
        <input
          placeholder="ID (Number)"
          type="number" // מבטיח שיוכלו להקליד רק מספרים
          value={id}
          onChange={(e) => setId(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />

        <button type="submit">Register</button>
        
        <div className="link-container">
          <span>Already have an account? </span>
          <button 
            type="button" 
            className="link-button" 
            onClick={() => navigate("/login")}
            style={{ background: 'none', border: 'none', color: 'blue', cursor: 'pointer', textDecoration: 'underline' }}
          >
            Login
          </button>
        </div>
      </form>
    </div>
  );
}