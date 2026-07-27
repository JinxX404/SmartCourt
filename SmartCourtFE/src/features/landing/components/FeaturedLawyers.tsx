import { LuChevronLeft, LuStar } from "react-icons/lu";
import { Link } from "react-router-dom";

const MOCK_LAWYERS = [
  {
    id: 1,
    name: "أحمد مصطفى",
    specialty: "قانون الشركات",
    initials: "أ.م",
    rating: "5.0",
    reviews: "60 توثيق",
    // Swapped to standard Tailwind grays
    avatarBg: "bg-gray-200",
    avatarText: "text-gray-600",
  },
  {
    id: 2,
    name: "سارة عبد العزيز",
    specialty: "الأحوال الشخصية",
    initials: "س.ع",
    rating: "4.8",
    reviews: "55 توثيق",
    avatarBg: "bg-gray-200",
    avatarText: "text-gray-600",
  },
  {
    id: 3,
    name: "كريم طارق",
    specialty: "الجنايات",
    initials: "ك.ط",
    rating: "4.7",
    reviews: "1.7 ألف توثيق",
    avatarBg: "bg-secondary-gray",
    avatarText: "text-white",
  },
];

export const FeaturedLawyers = () => {
  return (
    <section className="w-full bg-gray-50 py-20 flex justify-center">
      <div className="w-full max-w-7xl px-6 flex flex-col gap-10">
        
        <div className="flex flex-row justify-between items-center w-full">
          <div className="flex flex-col items-start gap-1">
            <h2 className="text-navy-light text-2xl font-bold">
              محامون مميزون
            </h2>
            <p className="text-gray-500 text-sm">
              نخبة من أفضل المحامين نتيجة في المنصة
            </p>
          </div>

          <Link 
            to="/lawyers" 
            className="flex flex-row items-center gap-2 text-gold hover:text-gold-hover transition-colors duration-200"
          >
            <span className="font-medium text-base">عرض الكل</span>
            <LuChevronLeft className="w-4 h-4" />
          </Link>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 w-full">
          {MOCK_LAWYERS.map((lawyer) => (
            <div 
              key={lawyer.id} 
              className="flex flex-col p-6 bg-white border border-gray-100 rounded-xl shadow-sm gap-4"
            >
              <div className="flex flex-row items-center gap-4 w-full">
                <div className={`w-16 h-16 rounded-full flex items-center justify-center shrink-0 ${lawyer.avatarBg}`}>
                  <span className={`font-bold text-xl ${lawyer.avatarText}`}>
                    {lawyer.initials}
                  </span>
                </div>

                <div className="flex flex-col items-start grow">
                  <h4 className="text-navy-light text-lg font-bold">
                    {lawyer.name}
                  </h4>
                  <span className="text-gray-500 text-xs mt-1">
                    {lawyer.specialty}
                  </span>
                </div>
              </div>

              <div className="flex flex-row justify-between items-center w-full border-t border-gray-200 pt-4 mt-2">
                <span className="text-gray-600 text-sm">
                  {lawyer.reviews}
                </span>
                
                <div className="flex flex-row items-center gap-1">
                  <span className="text-gold font-bold text-sm">
                    {lawyer.rating}
                  </span>
                  <LuStar className="w-4 h-4 text-gold fill-current" />
                </div>
              </div>
            </div>
          ))}
        </div>

      </div>
    </section>
  );
};