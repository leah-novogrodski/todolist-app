import axios from 'axios';

// הגדרת ה-URL הבסיסי
const API_URL = "https://authserver-wcxa.onrender.com";
axios.defaults.baseURL = API_URL;

// חובה להוסיף את זה כדי שיתאים ל-AllowCredentials בשרת!
axios.defaults.withCredentials = false; 

// הוספת הטוקן לכל בקשה
axios.interceptors.request.use(config => {
    const getCookie = (name) => {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    };

    const token = getCookie("token");
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// ... שאר ה-interceptor של התגובה נשאר אותו דבר ...

export default {
    register: async (name, password, id) => {
        // שולחים אובייקט שתואם למחלקת User ב-C#
        const result = await axios.post("/register", { name, password, id: Number(id) });
        return result.data;
    },
    login: async (name, password) => {
        const result = await axios.post("/login", { name, password });
        return result.data;
    },
    // ... שאר הפונקציות ...
};