import { LuChevronLeft, LuStar } from "react-icons/lu";
import { Link } from "react-router-dom";

const MOCK_LAWYERS = [
  {
    id: 1,
    name: "أحمد مصطفى",
    specialty: "قانون الشركات والاستثمار",
    initials: "أ.م",
    rating: "5.0",
    reviews: "60 توثيق",
  },
  {
    id: 2,
    name: "سارة عبد العزيز",
    specialty: "الأحوال الشخصية والارث",
    initials: "س.ع",
    rating: "4.8",
    reviews: "55 توثيق",
  },
  {
    id: 3,
    name: "كريم طارق",
    specialty: "القضايا الجنائية والعامة",
    initials: "ك.ط",
    rating: "4.7",
    reviews: "1.7 ألف توثيق",
  },
];

export const FeaturedLawyers = () => {
  return (
    <section className="w-full bg-bg-primary py-24 transition-colors duration-300">
      <div className="w-full max-w-7xl mx-auto px-6 flex flex-col gap-12">
        
        {/* Header */}
        <div className="flex flex-row justify-between items-end w-full">
          <div className="flex flex-col items-start gap-2.5 text-right">
            <span className="text-gold font-bold text-sm tracking-widest uppercase">شركاء النجاح</span>
            <h2 className="text-text-primary text-3xl md:text-4xl font-extrabold tracking-tight">
              محامون مميزون
            </h2>
            <p className="text-text-secondary text-sm md:text-base leading-relaxed">
              نخبة من أفضل المحامين المرخصين والمعتمدين لمتابعة قضيتك وتوجيهك.
            </p>
          </div>

          <Link 
            to="/lawyers" 
            className="flex items-center gap-1.5 text-gold hover:text-gold-hover font-semibold text-sm transition-colors duration-200"
          >
            <span>عرض الكل</span>
            <LuChevronLeft className="w-4 h-4" />
          </Link>
        </div>

        {/* Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {MOCK_LAWYERS.map((lawyer) => (
            <div 
              key={lawyer.id} 
              className="flex flex-col p-8 bg-bg-secondary border border-border-primary rounded-[24px] shadow-card hover:shadow-premium hover:-translate-y-1 transition-all duration-300 gap-6 text-right"
            >
              <div className="flex items-center gap-4 w-full">
                {/* Avatar */}
                <div className="w-14 h-14 rounded-2xl bg-gold/10 border border-gold/20 flex items-center justify-center shrink-0 text-gold font-bold text-lg">
                  {lawyer.initials}
                </div>

                {/* Name & Specialty */}
                <div className="flex flex-col items-start grow text-right">
                  <h4 className="text-text-primary text-lg font-bold leading-snug">
                    {lawyer.name}
                  </h4>
                  <span className="text-text-secondary text-xs mt-1">
                    {lawyer.specialty}
                  </span>
                </div>
              </div>

              {/* Rating and Reviews */}
              <div className="flex items-center justify-between w-full border-t border-border-primary pt-5 mt-1">
                <span className="text-text-secondary text-sm">
                  {lawyer.reviews}
                </span>
                
                <div className="flex items-center gap-1">
                  <span className="text-gold font-bold text-sm">
                    {lawyer.rating}
                  </span>
                  <LuStar className="w-4.5 h-4.5 text-gold fill-current" />
                </div>
              </div>
            </div>
          ))}
        </div>

      </div>
    </section>
  );
};