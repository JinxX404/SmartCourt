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
  if (spec === null || spec === undefined) return "تخصص عام";
  switch (spec) {
    case 0: return "أسرة";
    case 1: return "مدني";
    case 2: return "تجاري";
    case 3: return "إداري ومجلس دولة";
    case 4: return "جنائي";
    case 5: return "عمالي";
    case 6: return "دستوري";
    case 7: return "ضرائب";
    case 8: return "جمارك";
    case 9: return "شركات";
    case 10: return "عقود";
    case 11: return "ملكية فكرية";
    case 12: return "تحكيم";
    case 13: return "بنوك وتمويل";
    case 14: return "استثمار";
    case 15: return "عقاري وشهر عقاري";
    case 16: return "تنفيذ";
    case 17: return "تأمين";
    case 18: return "بيئة";
    case 19: return "تكنولوجيا معلومات واتصالات";
    case 20: return "جرائم إلكترونية";
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
              <div className="flex items-center gap-1.5 mt-2">
                <LuStar className="w-4 h-4 text-yellow-500 fill-yellow-500" />
                <span className="text-sm font-bold text-gray-700 dark:text-gray-300">
                  {lawyer.rating > 0 ? lawyer.rating.toFixed(1) : "جديد"}
                </span>
                {lawyer.rating > 0 && (
                  <span className="text-xs text-gray-400 font-normal mr-1">(تقييم عشوائي)</span>
                )}
              </div>
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
          <p className="text-gray-600 dark:text-gray-300 text-sm leading-relaxed line-clamp-2" title={lawyer.bio || ""}>
            {lawyer.bio ? (lawyer.bio.length > 100 ? lawyer.bio.substring(0, 100) + '...' : lawyer.bio) : "لا توجد نبذة تعريفية."}
          </p>
        </div>

        <div className="flex flex-wrap gap-2 mb-6">
          {lawyer.specializations && lawyer.specializations.length > 0 ? (
            <>
              {lawyer.specializations.slice(0, 2).map((spec, index) => (
                <span key={index} className="inline-flex items-center gap-1.5 text-xs font-semibold bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 px-3 py-1.5 rounded-full border border-gray-200 dark:border-gray-700">
                  <LuAward className="w-3.5 h-3.5 text-gold" />
                  {getSpecializationName(spec.specialization)}
                </span>
              ))}
              {lawyer.specializations.length > 2 && (
                <span className="inline-flex items-center gap-1 text-xs font-semibold bg-gold/10 text-gold px-3 py-1.5 rounded-full border border-gold/20" title="اضغط على عرض الملف الشخصي لمعرفة باقي التخصصات">
                  +{lawyer.specializations.length - 2} تخصص آخر
                </span>
              )}
            </>
          ) : (
            <span className="inline-flex items-center gap-1.5 text-xs font-semibold bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 px-3 py-1.5 rounded-full border border-gray-200 dark:border-gray-700">
              <LuAward className="w-3.5 h-3.5 text-gold" />
              {getSpecializationName(lawyer.specialization)}
            </span>
          )}
          <span className="inline-flex items-center gap-1.5 text-xs font-semibold bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 px-3 py-1.5 rounded-full border border-gray-200 dark:border-gray-700">
            <LuMapPin className="w-3.5 h-3.5 text-gold" />
            {lawyer.governorate || "لم يحدد"}
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
