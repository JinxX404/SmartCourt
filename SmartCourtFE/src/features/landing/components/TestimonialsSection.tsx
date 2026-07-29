import { LuQuote } from "react-icons/lu";

const REVIEWS = [
  {
    name: "عبد الرحمن بن سليمان",
    role: "رائد أعمال",
    content: "استخدمت المنصة لصياغة وتدقيق عقود التأسيس الخاصة بشركتي الناشئة. النتيجة كانت فورية ودقيقة ومطابقة تماماً للأنظمة السعودية. وفرت علي الكثير من الوقت والمال.",
    initials: "ع.س",
  },
  {
    name: "سارة القحطاني",
    role: "مستشارة قانونية",
    content: "كمحامية ومستشارة، أرى أن هذه المنصة أداة مساعدة رائعة تختصر ساعات من البحث في الأنظمة واللوائح وتسهل تحليل الثغرات في العقود المعقدة.",
    initials: "س.ق",
  },
  {
    name: "خالد الشمري",
    role: "مدير شركة عقارية",
    content: "خدمة الاستشارات الفورية عبر الذكاء الاصطناعي كانت منقذة لنا في قرارات سريعة جداً. دقة الإجابات مطابقة للواقع ومبنية على مصادر نظامية صحيحة.",
    initials: "خ.ش",
  },
];

export const TestimonialsSection = () => {
  return (
    <section className="w-full bg-bg-primary py-24 transition-colors duration-300">
      <div className="w-full max-w-7xl mx-auto px-6 flex flex-col gap-12">
        
        {/* Header */}
        <div className="flex flex-col items-center gap-3 text-center">
          <span className="text-gold font-bold text-sm tracking-widest uppercase">آراء عملائنا</span>
          <h2 className="text-text-primary text-3xl md:text-4xl font-extrabold tracking-tight">
            ماذا يقولون عن مستشار؟
          </h2>
          <p className="text-text-secondary text-sm md:text-base max-w-xl leading-relaxed">
            نفتخر بثقة آلاف الشركات والأفراد الذين يعتمدون على منصتنا لإنجاز أعمالهم وحماية مصالحهم القانونية.
          </p>
        </div>

        {/* Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-4">
          {REVIEWS.map((review, index) => (
            <div 
              key={index}
              className="flex flex-col justify-between p-8 bg-bg-secondary border border-border-primary rounded-[24px] shadow-card hover:shadow-premium transition-all duration-300 text-right gap-6 relative animate-fade-in"
            >
              {/* Quote icon decoration */}
              <div className="absolute top-6 left-6 text-gold/10 pointer-events-none">
                <LuQuote className="w-10 h-10 rotate-180" />
              </div>

              {/* Review text */}
              <p className="text-text-secondary text-sm md:text-base leading-relaxed relative z-10 text-right">
                "{review.content}"
              </p>

              {/* User Avatar & Info */}
              <div className="flex items-center gap-4 border-t border-border-primary pt-6 mt-2">
                {/* Avatar Placeholder */}
                <div className="w-12 h-12 bg-gold/10 border border-gold/30 rounded-full flex items-center justify-center text-gold font-bold shrink-0 text-sm">
                  {review.initials}
                </div>
                
                {/* User Info */}
                <div className="flex flex-col items-start grow text-right">
                  <h4 className="text-text-primary font-bold text-base leading-tight">
                    {review.name}
                  </h4>
                  <span className="text-text-secondary text-xs mt-1">
                    {review.role}
                  </span>
                </div>
              </div>

            </div>
          ))}
        </div>

      </div>
    </section>
  );
};
