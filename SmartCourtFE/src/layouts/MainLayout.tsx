import { useEffect, useState } from "react";
import { Outlet } from "react-router-dom";
import { Navbar } from "./Navbar"; 
import { Footer } from "./Footer";

export const MainLayout = () => {
  const [theme, setTheme] = useState<"light" | "dark">(() => {
    const saved = localStorage.getItem("theme");
    if (saved === "light" || saved === "dark") return saved;
    return "light";
  });

  useEffect(() => {
    const root = document.documentElement;
    if (theme === "dark") {
      root.classList.add("dark");
    } else {
      root.classList.remove("dark");
    }
    localStorage.setItem("theme", theme);
  }, [theme]);

  const toggleTheme = () => {
    setTheme((prev) => (prev === "light" ? "dark" : "light"));
  };

  return (
    <div className="flex flex-col min-h-screen w-full bg-bg-primary text-text-primary transition-colors duration-300">
      <Navbar theme={theme} toggleTheme={toggleTheme} />

      <div className="grow flex flex-col relative w-full">
        <Outlet context={{ theme }} />
      </div>

      <Footer /> 
    </div>
  );
};