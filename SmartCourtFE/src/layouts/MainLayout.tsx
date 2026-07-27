import { Outlet } from "react-router-dom";
import { Navbar } from "./Navbar"; 
import { Footer } from "./Footer";

export const MainLayout = () => {
  return (
    <div className="flex flex-col min-h-screen w-full bg-Surface text-navy">
      
      <Navbar />

      <div className="grow flex flex-col relative w-full">
        <Outlet />
      </div>

       <Footer /> 
      
    </div>
  );
};