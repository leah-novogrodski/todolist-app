import React, { useEffect, useState } from "react";
import service from "./service.js";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import { AuthProvider } from "./AuthProvider.js";
import Login from "./login.js";
import Register from "./Register.js";
import "./app.css";

// ✅ פונקציית עזר לקריאת קוקי - מחוץ לקומפוננטה
const getCookie = (name) => {
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) return parts.pop().split(";").shift();
  return null;
};

// ✅ PrivateRoute מחוץ ל-App - זו הייתה הבעיה המרכזית!
function PrivateRoute({ children }) {
  const token = getCookie("token");
  return token ? children : <Navigate to="/login" />;
}

// ✅ TodoItem מחוץ ל-App
function TodoItem({ todo, onToggle, onDelete }) {
  return (
    <li className={`todo-item ${todo.isComplete ? "completed" : ""}`}>
      <div className="view">
        <input
          type="checkbox"
          checked={todo.isComplete}
          onChange={(e) => onToggle(todo, e.target.checked)}
        />
        <label>{todo.name}</label>
        <button className="destroy" onClick={() => onDelete(todo.id)}></button>
      </div>
    </li>
  );
}

function App() {
  const [newTodo, setNewTodo] = useState("");
  const [todos, setTodos] = useState([]);

  async function getTodos() {
    const todos = await service.getTasks();
    setTodos(todos);
  }

  useEffect(() => {
    const token = getCookie("token");
    if (token) {
      // ✅ רק אם יש טוקן
      getTodos();
    }
  }, []);

  async function createTodo(e) {
    e.preventDefault();
    if (!newTodo.trim()) return;
    await service.addTask(newTodo);
    setNewTodo("");
    await getTodos();
  }

  async function updateCompleted(todo, isComplete) {
    await service.setCompleted(todo.id, isComplete, todo.name); // ✅ מעביר את השם
    await getTodos();
  }

  async function deleteTodo(id) {
    await service.deleteTask(id);
    await getTodos();
  }

  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route
            path="/"
            element={
              <PrivateRoute>
                <div className="todoapp">
                  <h1>Todos</h1>
                  <form onSubmit={createTodo}>
                    <input
                      className="new-todo"
                      placeholder="What needs to be done?"
                      value={newTodo}
                      onChange={(e) => setNewTodo(e.target.value)}
                    />
                  </form>
                  <ul className="todo-list">
                    {todos.map((todo) => (
                      <TodoItem
                        key={todo.id}
                        todo={todo}
                        onToggle={updateCompleted}
                        onDelete={deleteTodo}
                      />
                    ))}
                  </ul>
                </div>
              </PrivateRoute>
            }
          />
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
