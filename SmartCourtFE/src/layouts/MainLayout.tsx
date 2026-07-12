import React from 'react';
import { Navbar } from './Navbar';

interface MainLayoutProps {
  children: React.ReactNode;
}

export const MainLayout = ({ children }: MainLayoutProps) => {
  return (
    <div className="min-h-screen flex flex-col bg-surface font-sans">
      <Navbar />
      
      <main className="grow">
        {children}
      </main>

     
    </div>
  );
};