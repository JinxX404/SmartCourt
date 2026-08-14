import { useOutletContext } from "react-router-dom";
import { LuSearch, LuStar, LuShieldCheck, LuAward } from "react-icons/lu";

const HERO_FEATURES = [
  {
    icon: LuStar,
    title: "تصنيفات حقيقية وموثوقة",
  },
  {
    icon: LuShieldCheck,
    title: "دفع آمن بالضمان الكامل",
  },
  {
    icon: LuAward,
    title: "نخبة من المحامين المرخصين",
  },
];

export const HeroSection = () => {
  const { theme } = useOutletContext<{ theme: "light" | "dark" }>();

  return (
    <section className="relative w-full overflow-hidden bg-bg-hero py-10 md:py-16 transition-colors duration-300 min-h-0 lg:min-h-[calc(100vh-80px)] flex items-center">

      {/* Background soft glowing circles for premium vibe */}
      <div className="absolute top-0 right-1/4 w-96 h-96 rounded-full bg-gold/5 blur-3xl -z-10 pointer-events-none"></div>
      <div className="absolute bottom-0 left-1/4 w-[500px] h-[500px] rounded-full bg-gold/5 blur-3xl -z-10 pointer-events-none"></div>

      <div className="w-full max-w-7xl mx-auto px-6 flex flex-col gap-8 md:gap-12 lg:gap-16">

        {/* Two-Column Layout */}
        <div className="grid grid-cols-1 md:grid-cols-12 gap-8 md:gap-6 lg:gap-8 items-center w-full">

          {/* Right side: Headlines, Buttons, and Search */}
          <div className="md:col-span-6 lg:col-span-6 flex flex-col items-start text-right gap-5 md:gap-4 lg:gap-8 z-10">
            <div className="flex flex-col gap-3 md:gap-3 lg:gap-4">

              <h1 className="text-text-primary text-3xl sm:text-4xl md:text-2xl lg:text-5xl font-extrabold leading-tight tracking-tight">
                مرحباً بك في المنصة القانونية
                <br />
                <span className="text-gold mt-1.5 md:mt-1 lg:mt-2 block">
                  الذكية والأولى
                </span>
              </h1>

              <p className="text-text-secondary text-sm sm:text-base md:text-sm lg:text-base leading-relaxed max-w-xl">
                استشارات قانونية دقيقة وسريعة مدعومة بأحدث تقنيات الذكاء الاصطناعي لحماية مصالحك وصياغة أعمالك القانونية بكل أمان وموثوقية.
              </p>
            </div>

            <div className="flex flex-row gap-3 sm:gap-4 w-full sm:w-auto">
              <button className="flex-1 sm:flex-initial px-5 py-3 md:px-4 md:py-2.5 lg:px-8 lg:py-3.5 bg-gold hover:bg-gold-hover text-white font-bold text-xs md:text-xs lg:text-base rounded-xl transition-all shadow-premium hover:shadow-none hover:translate-y-0.5 cursor-pointer whitespace-nowrap">
                ابدأ استشارتك الآن
              </button>
              <button className="flex-1 sm:flex-initial px-5 py-3 md:px-4 md:py-2.5 lg:px-8 lg:py-3.5 bg-transparent border-2 border-gold/30 hover:border-gold text-text-primary font-bold text-xs md:text-xs lg:text-base rounded-xl transition-all cursor-pointer whitespace-nowrap">
                اعرف المزيد
              </button>
            </div>

            {/* Smart Search Bar */}
            <div className="w-full max-w-md relative mt-2 md:mt-2 lg:mt-4 group">
              <div className="absolute inset-0 bg-gold/20 rounded-xl blur-xl group-hover:blur-2xl transition-all duration-500 opacity-0 group-hover:opacity-100"></div>
              <div className="relative flex items-center bg-bg-secondary border border-border-primary rounded-xl p-1.5 sm:p-2 shadow-card focus-within:border-gold/50 transition-colors">
                <input
                  type="text"
                  placeholder="اكتب سؤالك القانوني هنا..."
                  className="w-full bg-transparent border-none outline-none text-text-primary placeholder:text-text-secondary/50 px-3 sm:px-4 font-medium text-xs md:text-xs lg:text-sm"
                />
                <button className="h-9 sm:h-10 lg:h-11 px-4 sm:px-5 bg-gold hover:bg-gold-hover text-white font-bold text-xs md:text-xs lg:text-sm rounded-xl flex items-center gap-2 transition-all shrink-0 cursor-pointer">
                  <span>بحث</span>
                  <LuSearch className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
                </button>
              </div>
            </div>
          </div>

          {/* Left side: Premium Seamless Illustration (Hidden completely on mobile, visible on tablet & desktop) */}
          <div className="hidden md:flex md:col-span-6 lg:col-span-6 items-center justify-center relative w-full h-full min-h-[300px] md:min-h-[380px] lg:min-h-[500px]">
            <img
              src={theme === "light" ? "/hero_final-nobg.png" : "/DarkModeimg2.png"}
              alt="الميزان القانوني الذكي"
              className="w-full max-h-[360px] md:max-h-[420px] lg:max-h-[550px] object-contain transition-all duration-500 ease-in-out hover:scale-[1.05] select-none"
            />
          </div>

        </div>

        {/* 4 Premium Soft Cards at the Bottom */}
        {/* 3 Trust Features at the Bottom */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-6 w-full mt-4">
          {HERO_FEATURES.map((feature, index) => {
            const Icon = feature.icon;
            return (
              <div
                key={index}
                className="group flex items-center gap-4 p-5 bg-bg-secondary border border-border-primary rounded-xl shadow-card hover:shadow-premium hover:-translate-y-1 transition-all duration-300 text-right cursor-pointer animate-fade-in"
              >
                {/* Icon wrapper */}
                <div className="w-12 h-12 bg-gold/10 border border-gold/20 rounded-xl flex items-center justify-center text-gold shrink-0 transition-all duration-300 group-hover:bg-gold group-hover:text-white">
                  <Icon className="w-5.5 h-5.5" />
                </div>

                {/* Content */}
                <div className="flex flex-col justify-center h-full">
                  <h4 className="text-text-primary font-bold text-base leading-snug group-hover:text-gold transition-colors">
                    {feature.title}
                  </h4>
                </div>
              </div>
            );
          })}
        </div>

      </div>
    </section>
  );
};