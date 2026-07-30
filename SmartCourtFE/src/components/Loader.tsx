import { DotLottieReact } from '@lottiefiles/dotlottie-react';

interface LoaderProps {
  fullScreen?: boolean;
  size?: number;
}

export const Loader = ({ fullScreen = true, size = 200 }: LoaderProps) => {
  const containerClasses = fullScreen
    ? "fixed inset-0 z-50 flex flex-col items-center justify-center bg-bg-primary transition-all duration-500"
    : "flex flex-col items-center justify-center p-6 w-full h-full";

  return (
    <div className={containerClasses}>
      <div 
        style={{ width: `${size}px`, height: `${size}px` }} 
        className="relative flex items-center justify-center"
      >
        <DotLottieReact
          src="https://lottie.host/bf4709a8-6961-4a67-abac-bf93f7e2b05a/2U5LIn4tUj.lottie"
          loop
          autoplay
        />
      </div>
      
      {/* Premium subtitled loading text */}
      {fullScreen && (
        <div className="mt-4 flex flex-col items-center gap-1.5 animate-pulse">
          <p className="text-xl font-bold text-navy dark:text-gold tracking-wider">مستشار</p>
          <p className="text-xs text-gray-400 dark:text-gray-500">جاري تحميل المنصة الذكية...</p>
        </div>
      )}
    </div>
  );
};
