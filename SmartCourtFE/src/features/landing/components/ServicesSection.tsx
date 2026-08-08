import { LuScale, LuFileText, LuCpu, LuShieldCheck } from "react-icons/lu";

const SERVICES = [
  {
    icon: LuScale,
    title: "استشارة قانونية",
    description: "احصل على توجيه قانوني سريع ودقيق لمعاملاتك من خلال الذكاء الاصطناعي.",
  },
  {
    icon: LuFileText,
    title: "صياغة العقود",
    description: "أنشئ وراجع العقود القانونية بسهولة ودقة متناهية متوافقة مع الأنظمة.",
  },
  {
    icon: LuCpu,
    title: "تحليل المستندات",
    description: "حلل الملفات والوثائق القانونية واستخلص الثغرات والنقاط الهامة عبر الـ AI.",
  },
  {
    icon: LuShieldCheck,
    title: "حماية حقوقك",
    description: "احصل على حلول وتوصيات قانونية موثوقة تضمن حماية متكاملة لمصالحك.",
  },
];

export const ServicesSection = () => {
  return (
    <section id="services" className="w-full bg-bg-primary py-24 transition-colors duration-300">
      <div className="w-full max-w-7xl mx-auto px-6 flex flex-col gap-12">
        
        {/* Header */}
        <div className="flex flex-col items-center gap-3 text-center">
          <span className="text-gold font-bold text-sm tracking-widest uppercase">خدماتنا الذكية</span>
          <h2 className="text-text-primary text-3xl md:text-4xl font-extrabold tracking-tight">
            حلول قانونية متكاملة بضغطة زر
          </h2>
          <p className="text-text-secondary text-sm md:text-base max-w-2xl leading-relaxed">
            نجمع بين عمق الخبرة القانونية وسرعة الذكاء الاصطناعي لنقدم لك أفضل تجربة حماية ودعم قانوني.
          </p>
        </div>

        {/* Grid Layout */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mt-4">
          {SERVICES.map((service, index) => {
            const Icon = service.icon;
            return (
              <div 
                key={index}
                className="group flex flex-col p-8 bg-bg-secondary border border-border-primary rounded-[24px] shadow-card hover:shadow-premium hover:-translate-y-1 transition-all duration-300 text-right gap-6 relative overflow-hidden"
              >
                {/* Thin gold top bar on hover */}
                <div className="absolute top-0 right-0 left-0 h-1 bg-gold scale-x-0 group-hover:scale-x-100 transition-transform duration-300 origin-right"></div>
                
                {/* Icon wrapper */}
                <div className="w-14 h-14 bg-gold/5 border border-gold/25 rounded-2xl flex items-center justify-center text-gold transition-all duration-300 group-hover:bg-gold group-hover:text-white shrink-0">
                  <Icon className="w-6.5 h-6.5" />
                </div>

                {/* Content */}
                <div className="flex flex-col gap-2">
                  <h3 className="text-text-primary font-bold text-xl leading-relaxed group-hover:text-gold transition-colors">
                    {service.title}
                  </h3>
                  <p className="text-text-secondary text-sm leading-relaxed">
                    {service.description}
                  </p>
                </div>
              </div>
            );
          })}
        </div>

      </div>
    </section>
  );
};
