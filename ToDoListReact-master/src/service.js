import axios from 'axios';

// 1. הגדרות בסיס
axios.defaults.baseURL = "http://localhost:5092";

// 2. הוספת הטוקן אוטומטית לכל בקשה
axios.interceptors.request.use(config => {
  const getCookie = (name) => {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
  };

  const token = getCookie("token"); // ✅ קורא מה-cookie ולא מ-localStorage
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 3. ✅ Interceptor מתוקן - בלי לולאה אינסופית
axios.interceptors.response.use(
  res => res,
  err => {
    if (err.response && err.response.status === 401) {
      // ✅ רק אם לא כבר בדף login
      if (!window.location.pathname.includes("/login")) {
        document.cookie = "token=; path=/; max-age=0"; // מחיקת הטוקן
        window.location.href = "/login";
      }
    }
    return Promise.reject(err);
  }
);

// eslint-disable-next-line import/no-anonymous-default-export
export default {
  register: async (name, password, id) => {
    const result = await axios.post("/register", { name, password, id });
    return result.data;
  },

  login: async (name, password) => {
    const result = await axios.post("/login", { name, password });
    return result.data;
  },

  getTasks: async () => {
    const result = await axios.get("/todos");
    return result.data;
  },

  addTask: async (name) => {
    const result = await axios.post("/todos", {
      name: name,
      isComplete: false
    });
    return result.data;
  },
  setCompleted: async (id, isComplete, name) => {
  // ✅ שולח את השם ישירות - אין צורך ב-GET נוסף
  const result = await axios.put(`/todos/${id}`, {
    name: name,
    isComplete: isComplete
  });
  return result.data;
},

  deleteTask: async (id) => {
    await axios.delete(`/todos/${id}`);
  },

};