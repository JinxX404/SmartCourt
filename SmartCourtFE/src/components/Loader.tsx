import { DotLottieReact } from '@lottiefiles/dotlottie-react';

interface LoaderProps {
  fadeOut?: boolean;
  size?: number;
}

export const Loader = ({ fadeOut = false, size = 180 }: LoaderProps) => {
  return (
    <div
      className={`fixed inset-0 z-50 flex flex-col items-center justify-center transition-all duration-700 ease-in-out
        ${fadeOut 
          ? "opacity-0 scale-105 blur-md pointer-events-none" 
          : "opacity-100 scale-100 blur-none"
        }
        bg-[#f4f6f8] dark:bg-[#0c0e12]
      `}
    >
      {/* Decorative Radial Glows */}
      <div className="absolute inset-0 z-0 pointer-events-none overflow-hidden">
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[500px] h-[500px] rounded-full 
          bg-[radial-gradient(circle,rgba(212,175,55,0.08)_0%,transparent_70%)] 
          dark:bg-[radial-gradient(circle,rgba(212,175,55,0.12)_0%,transparent_70%)]"
        />
        <div className="absolute top-[-10%] right-[-10%] w-[300px] h-[300px] rounded-full 
          bg-navy/5 dark:bg-gold/5 blur-[80px]" 
        />
        <div className="absolute bottom-[-10%] left-[-10%] w-[300px] h-[300px] rounded-full 
          bg-gold/5 dark:bg-navy/5 blur-[80px]" 
        />
      </div>

      {/* Loader Content Wrapper */}
      <div className="relative z-10 flex flex-col items-center max-w-xs w-full mx-4 transition-all duration-500">
        {/* Lottie Animation Wrapper with mode-specific filter logic if needed */}
        <div 
          style={{ width: `${size}px`, height: `${size}px` }} 
          className="relative flex items-center justify-center transition-all duration-300
            filter drop-shadow-[0_4px_12px_rgba(212,175,55,0.15)]
            dark:drop-shadow-[0_4px_20px_rgba(212,175,55,0.3)]
          "
        >
          <DotLottieReact
            src="https://lottie.host/bf4709a8-6961-4a67-abac-bf93f7e2b05a/2U5LIn4tUj.lottie"
            loop
            autoplay
          />
        </div>
        
        {/* Animated Premium Typography */}
        <div className="mt-6 flex flex-col items-center gap-2 text-center">
          <h1 className="text-2xl font-black tracking-widest bg-gradient-to-r from-navy via-gold to-navy dark:from-white dark:via-gold dark:to-white bg-clip-text text-transparent animate-pulse">
            مُسْتَشَار
          </h1>
          
          {/* Subtitle with typing dots */}
          <div className="flex items-center gap-1.5 text-xs text-gray-500 dark:text-gray-400 font-medium">
            <span>جاري تهيئة المنصة القانونية الذكية</span>
            <span className="flex gap-0.5 mt-1">
              <span className="w-1 h-1 rounded-full bg-gold animate-bounce [animation-delay:-0.3s]"></span>
              <span className="w-1 h-1 rounded-full bg-gold animate-bounce [animation-delay:-0.15s]"></span>
              <span className="w-1 h-1 rounded-full bg-gold animate-bounce"></span>
            </span>
          </div>
        </div>
      </div>
      
      {/* Footer Branding */}
      <div className="absolute bottom-8 left-0 right-0 text-center z-10">
        <p className="text-[10px] uppercase tracking-[0.2em] text-gray-400 dark:text-gray-600 font-bold">
          Smart Court Legal Tech © 2026
        </p>
      </div>
    </div>
  );
};
