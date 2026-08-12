import React from "react";
import { Link } from "react-router-dom";
import { LuChevronLeft, LuShieldCheck, LuMapPin, LuStar, LuAward } from "react-icons/lu";
import type { LawyerSearchDto } from "../types";

interface LawyerCardProps {
  lawyer: LawyerSearchDto;
}

const getLevelName = (level: number) => {
  switch (level) {
    case 1:
      return "جدول عام";
    case 2:
      return "ابتدائي";
    case 3:
      return "استئناف";
    case 4:
      return "نقض";
    case 5:
      return "مستشار قانوني";
    default:
      return "محامي";
  }
};

const getSpecializationName = (spec: number | null) => {
  if (!spec) return "تخصص عام";
  switch (spec) {
    case 1: return "جنائي";
    case 2: return "مدني";
    case 3: return "أسرة";
    case 4: return "شركات";
    case 5: return "عقاري";
    case 6: return "إداري";
    case 7: return "جرائم إلكترونية";
    default: return "تخصص عام";
  }
};


export const LawyerCard: React.FC<LawyerCardProps> = ({ lawyer }) => {
  return (
    <div className="bg-white dark:bg-navy border border-gray-200 dark:border-border-primary rounded-xl overflow-hidden shadow-sm transition-transform hover:-translate-y-1 hover:shadow-md group">
      <div className="p-6">
        <div className="flex justify-between items-start mb-5">
          <div className="flex items-center gap-4">
            <div className="w-20 h-20 md:w-24 md:h-24 rounded-full overflow-hidden bg-gray-100 dark:bg-gray-800 flex items-center justify-center border-2 border-gold/30 shadow-sm shrink-0">
              {lawyer.profilePictureUrl ? (
                <img
                  src={lawyer.profilePictureUrl}
                  alt={lawyer.name}
                  className="w-full h-full object-cover"
                />
              ) : (
                <span className="text-xl font-bold text-gold">
                  {lawyer.name.charAt(0)}
                </span>
              )}
            </div>
            <div>
              <h3 className="text-xl font-bold text-gray-900 dark:text-white group-hover:text-gold transition-colors">
                {lawyer.name}
              </h3>
              <p className="text-sm font-semibold text-gray-500 dark:text-gray-400 mt-1 flex items-center gap-1.5">
                <LuShieldCheck className="w-4 h-4 text-gold" />
                محامي {getLevelName(lawyer.level)}
              </p>
            </div>
          </div>
          
          <div className="flex flex-col items-end gap-1">
            {lawyer.isAvailable && (
              <span className="text-[10px] bg-green-500/10 text-green-600 dark:text-green-400 px-2 py-0.5 rounded-full font-medium">
                متاح حالياً
              </span>
            )}
          </div>
        </div>

        <div className="mb-6">
          <p className="text-gray-600 dark:text-gray-300 text-sm leading-relaxed line-clamp-2">
            {lawyer.bio || "لا توجد نبذة تعريفية."}
          </p>
        </div>

        <div className="flex flex-wrap gap-2 mb-6">
          <span className="inline-flex items-center gap-1.5 text-xs font-semibold bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 px-3 py-1.5 rounded-full border border-gray-200 dark:border-gray-700">
            <LuAward className="w-3.5 h-3.5 text-gold" />
            {getSpecializationName(lawyer.specialization)}
          </span>
          <span className="inline-flex items-center gap-1.5 text-xs font-semibold bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 px-3 py-1.5 rounded-full border border-gray-200 dark:border-gray-700">
            <LuMapPin className="w-3.5 h-3.5 text-gold" />
            {lawyer.governorate || "لم يحدد"}
          </span>
          <span className="inline-flex items-center gap-1.5 text-xs font-semibold bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 px-3 py-1.5 rounded-full border border-gray-200 dark:border-gray-700">
            <LuStar className="w-3.5 h-3.5 text-yellow-500 fill-yellow-500" />
            {lawyer.rating ? lawyer.rating.toFixed(1) : "جديد"}
          </span>
        </div>


        <Link
          to={`/lawyers/${lawyer.id}`}
          className="w-full py-2.5 rounded-lg border border-gold/50 text-gold font-medium text-sm flex items-center justify-center gap-2 hover:bg-gold hover:text-white dark:hover:text-navy transition-all duration-300"
        >
          عرض الملف الشخصي
          <LuChevronLeft className="w-4 h-4" />
        </Link>
      </div>
    </div>
  );
};
