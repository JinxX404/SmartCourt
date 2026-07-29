import { LuLandmark, LuShieldCheck, LuUsers } from "react-icons/lu";

export const TrustBar = () => {
  return (
    <div className="w-full bg-bg-secondary border-y border-border-primary py-6 flex flex-col items-center justify-center transition-colors duration-300">
      <div className="w-full max-w-7xl px-6 flex flex-row flex-wrap justify-center items-center gap-y-4 md:gap-y-0 gap-x-12 text-text-primary text-sm font-medium">

        <div className="flex flex-row items-center gap-3 justify-center">
          <div className="w-8 h-8 rounded-lg bg-gold/10 flex items-center justify-center text-gold">
            <LuLandmark className="w-4.5 h-4.5 shrink-0" />
          </div>
          <span>تصنيفات حقيقية وموثوقة</span>
        </div>

        <div className="hidden md:block w-1.5 h-1.5 rounded-full bg-border-primary"></div>

        <div className="flex flex-row items-center gap-3 justify-center">
          <div className="w-8 h-8 rounded-lg bg-gold/10 flex items-center justify-center text-gold">
            <LuShieldCheck className="w-4.5 h-4.5 shrink-0" />
          </div>
          <span>دفع آمن بالضمان الكامل</span>
        </div>

        <div className="hidden md:block w-1.5 h-1.5 rounded-full bg-border-primary"></div>

        <div className="flex flex-row items-center gap-3 justify-center">
          <div className="w-8 h-8 rounded-lg bg-gold/10 flex items-center justify-center text-gold">
            <LuUsers className="w-4.5 h-4.5 shrink-0" />
          </div>
          <span>نخبة من المحامين المرخصين</span>
        </div>

      </div>
    </div>
  );
};