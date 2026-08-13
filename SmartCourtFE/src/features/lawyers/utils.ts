export const getLevelName = (level: number) => {
  switch (level) {
    case 1: return "جدول عام";
    case 2: return "ابتدائي";
    case 3: return "استئناف";
    case 4: return "نقض";
    default: return "غير محدد";
  }
};

export const getSpecializationName = (spec: number | null) => {
  if (spec === null) return "استشارات عامة";
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
    default: return "استشارات عامة";
  }
};
