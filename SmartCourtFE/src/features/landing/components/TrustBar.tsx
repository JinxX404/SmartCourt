import { LuLandmark, LuShieldCheck, LuUsers } from "react-icons/lu";

export const TrustBar = () => {
  return (
    // Main wrapper matching Figma's background and padding
    <div className="w-full bg-gold py-4 flex flex-col items-center justify-center relative z-20">
      
      <div className="w-full max-w-7xl px-6 flex flex-row flex-wrap justify-center items-center gap-y-4 md:gap-y-0 text-navy">
        
        <div className="flex flex-row items-center gap-2 pl-0 md:pl-8 md:border-l border-navy/20 w-full md:w-auto justify-center">
          <LuLandmark className="w-5 h-5 shrink-0" />
          <span className="font-semibold text-md">تصنيفات حقيقية</span>
        </div>

        <div className="flex flex-row items-center gap-2 px-0 md:px-8 md:border-l border-navy/20 w-full md:w-auto justify-center">
          <LuShieldCheck className="w-5 h-5 shrink-0" />
          <span className="font-semibold text-md">دفع آمن بالضمان</span>
        </div>

        <div className="flex flex-row items-center gap-2 pr-0 md:pr-8 w-full md:w-auto justify-center">
          <LuUsers className="w-5 h-5 shrink-0" />
          <span className="font-semibold text-md">محامون مراقبون بعناية</span>
        </div>

      </div>
    </div>
  );
};