import React, { useState, useEffect } from "react";
import { motion } from "framer-motion";
import { LuSearch, LuSlidersHorizontal, LuScale } from "react-icons/lu";
import { useSearchLawyers } from "../features/lawyers/hooks/useSearchLawyers";
import { LawyerCard } from "../features/lawyers/components/LawyerCard";
import { SearchableSelect } from "../components/SearchableSelect";

const governorateOptions = [
  "القاهرة", "الجيزة", "الإسكندرية", "الدقهلية", "البحر الأحمر", "البحيرة", "الفيوم",
  "الغربية", "الإسماعيلية", "المنوفية", "المنيا", "القليوبية", "الوادي الجديد", "السويس",
  "أسوان", "أسيوط", "بني سويف", "بورسعيد", "دمياط", "الشرقية", "جنوب سيناء",
  "كفر الشيخ", "مطروح", "الأقصر", "قنا", "شمال سيناء", "سوهاج"
].map(g => ({ value: g, label: g }));

const specializationOptions = [
  { value: 0, label: "أسرة" },
  { value: 1, label: "مدني" },
  { value: 2, label: "تجاري" },
  { value: 3, label: "إداري ومجلس دولة" },
  { value: 4, label: "جنائي" },
  { value: 5, label: "عمالي" },
  { value: 6, label: "دستوري" },
  { value: 7, label: "ضرائب" },
  { value: 8, label: "جمارك" },
  { value: 9, label: "شركات" },
  { value: 10, label: "عقود" },
  { value: 11, label: "ملكية فكرية" },
  { value: 12, label: "تحكيم" },
  { value: 13, label: "بنوك وتمويل" },
  { value: 14, label: "استثمار" },
  { value: 15, label: "عقاري وشهر عقاري" },
  { value: 16, label: "تنفيذ" },
  { value: 17, label: "تأمين" },
  { value: 18, label: "بيئة" },
  { value: 19, label: "تكنولوجيا معلومات واتصالات" },
  { value: 20, label: "جرائم إلكترونية" }
];

export const LawyersPage = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [searchInput, setSearchInput] = useState("");
  
  const [level, setLevel] = useState<number | undefined>(undefined);
  const [specialization, setSpecialization] = useState<number | undefined>(undefined);
  const [governorate, setGovernorate] = useState<string>("");
  const [minRating, setMinRating] = useState<number | undefined>(undefined);
  const [isAvailable, setIsAvailable] = useState<boolean | undefined>(undefined);
  const [showFilters, setShowFilters] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);

  const { data, isLoading, isError } = useSearchLawyers({
    searchTerm,
    level,
    specialization,
    governorate: governorate || undefined,
    minRating,
    isAvailable,
    pageNumber,
    pageSize: 12,
  });

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchTerm !== searchInput) {
        setSearchTerm(searchInput);
        setPageNumber(1);
      }
    }, 500);
    return () => clearTimeout(timer);
  }, [searchInput, searchTerm]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchTerm(searchInput);
    setPageNumber(1);
  };

  return (
    <main className="flex-1 p-4 sm:p-8 overflow-y-auto w-full space-y-6 bg-gray-50/50 dark:bg-transparent">
      <div className="max-w-7xl mx-auto space-y-6">
        
        {/* Header Section */}
        <div className="text-center space-y-4">
          <div className="inline-flex items-center justify-center p-3 bg-gold/10 rounded-full mb-2">
            <LuScale className="w-8 h-8 text-gold" />
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white">
            دليل المحامين الشامل
          </h1>
          <p className="text-gray-500 dark:text-gray-400 max-w-2xl mx-auto text-sm">
            ابحث عن أفضل المحامين والمستشارين القانونيين المعتمدين وتواصل معهم مباشرة لمساعدتك في قضاياك واستشاراتك.
          </p>
        </div>

        {/* Search & Filter Bar */}
        <div className="bg-white dark:bg-navy border border-gray-200 dark:border-border-primary rounded-2xl p-5 shadow-sm">
          <form onSubmit={handleSearch} className="space-y-4">
            
            <div className="flex flex-col md:flex-row gap-3">
              <div className="flex-1 relative">
                <div className="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none">
                  <LuSearch className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  type="text"
                  placeholder="ابحث بالاسم، التخصص، أو الكلمات المفتاحية..."
                  className="w-full bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white rounded-xl py-3 pr-12 pl-4 focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold transition-all"
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                />
              </div>

              <button
                type="button"
                onClick={() => setShowFilters(!showFilters)}
                className={`px-6 py-3 rounded-xl transition-colors flex items-center justify-center gap-2 border font-medium ${showFilters ? 'bg-gold/10 text-gold border-gold/30' : 'bg-gray-50 dark:bg-gray-800/50 text-gray-700 dark:text-gray-300 border-gray-200 dark:border-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800'}`}
              >
                <LuSlidersHorizontal className="w-5 h-5" />
                فلاتر متقدمة
              </button>
            </div>

            {/* Advanced Filters Panel */}
            {showFilters && (
              <motion.div 
                initial={{ height: 0, opacity: 0 }}
                animate={{ height: "auto", opacity: 1 }}
                exit={{ height: 0, opacity: 0 }}
                className="pt-4 border-t border-gray-100 dark:border-gray-800 overflow-hidden"
              >
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                <select 
                  className="flex-1 bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white rounded-xl py-3 px-4 focus:outline-none focus:border-gold"
                  value={level || ""}
                  onChange={(e) => {
                    setLevel(e.target.value ? Number(e.target.value) : undefined);
                    setPageNumber(1);
                  }}
                >
                  <option value="">جميع الدرجات</option>
                  <option value="1">جدول عام</option>
                  <option value="2">ابتدائي</option>
                  <option value="3">استئناف</option>
                  <option value="4">نقض</option>
                </select>

                <SearchableSelect
                  options={specializationOptions}
                  value={specialization}
                  placeholder="جميع التخصصات"
                  onChange={(val) => {
                    setSpecialization(val as number | undefined);
                    setPageNumber(1);
                  }}
                />
                
                <SearchableSelect
                  options={governorateOptions}
                  value={governorate}
                  placeholder="كل المحافظات"
                  onChange={(val) => {
                    setGovernorate(val as string || "");
                    setPageNumber(1);
                  }}
                />
              </div>
              
              <div className="flex gap-6 mt-4 items-center flex-wrap px-2">
                <label className="flex items-center gap-2 text-sm text-gray-700 dark:text-gray-300 cursor-pointer">
                  <input 
                    type="checkbox" 
                    className="w-4 h-4 text-gold bg-gray-100 border-gray-300 rounded focus:ring-gold"
                    checked={isAvailable || false}
                    onChange={(e) => {
                      setIsAvailable(e.target.checked ? true : undefined);
                      setPageNumber(1);
                    }}
                  />
                  متاح حالياً فقط
                </label>
                
                <label className="flex items-center gap-2 text-sm text-gray-700 dark:text-gray-300 cursor-pointer">
                  <input 
                    type="checkbox" 
                    className="w-4 h-4 text-gold bg-gray-100 border-gray-300 rounded focus:ring-gold"
                    checked={minRating === 4}
                    onChange={(e) => {
                      setMinRating(e.target.checked ? 4 : undefined);
                      setPageNumber(1);
                    }}
                  />
                  تقييم 4 نجوم فأكثر
                </label>
              </div>
              </motion.div>
            )}
          </form>
        </div>

        {/* Results Section */}
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
              <LuSlidersHorizontal className="w-5 h-5 text-gold" />
              النتائج ({data?.totalRecords || 0})
            </h2>
          </div>

          {isLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 lg:gap-8">
              {[...Array(6)].map((_, i) => (
                <div key={i} className="bg-gray-100 dark:bg-navy border border-gray-200 dark:border-border-primary rounded-xl h-64 animate-pulse"></div>
              ))}
            </div>
          ) : isError ? (
            <div className="text-center py-12 bg-white dark:bg-navy border border-red-200 dark:border-red-900 rounded-xl">
              <p className="text-red-400">حدث خطأ أثناء جلب البيانات. يرجى المحاولة مرة أخرى.</p>
            </div>
          ) : data?.data && data.data.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 lg:gap-8">
              {data.data.map((lawyer, index) => (
                <motion.div 
                  key={lawyer.id}
                  initial={{ opacity: 0, y: 30 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.4, delay: index * 0.1, ease: "easeOut" }}
                >
                  <LawyerCard lawyer={lawyer} />
                </motion.div>
              ))}
            </div>
          ) : (
            <div className="text-center py-12 bg-white dark:bg-navy border border-gray-200 dark:border-border-primary rounded-xl">
              <LuSearch className="w-12 h-12 text-gray-500 mx-auto mb-4" />
              <p className="text-gray-400">لم يتم العثور على محامين يطابقون معايير البحث.</p>
            </div>
          )}

          {/* Pagination Controls */}
          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 pt-8">
              <button
                onClick={() => setPageNumber(p => Math.max(1, p - 1))}
                disabled={!data.hasPreviousPage}
                className="px-4 py-2 text-sm font-bold rounded-lg border border-gray-200 dark:border-gray-700 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-900 dark:text-white transition-colors"
              >
                السابق
              </button>
              
              <div className="flex items-center gap-1">
                {[...Array(data.totalPages)].map((_, i) => (
                  <button
                    key={i + 1}
                    onClick={() => setPageNumber(i + 1)}
                    className={`w-10 h-10 rounded-lg text-sm font-bold flex items-center justify-center transition-colors ${
                      pageNumber === i + 1
                        ? "bg-gold text-white shadow-sm"
                        : "hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-600 dark:text-gray-300 border border-transparent hover:border-gray-200 dark:hover:border-gray-700"
                    }`}
                  >
                    {i + 1}
                  </button>
                ))}
              </div>

              <button
                onClick={() => setPageNumber(p => Math.min(data.totalPages, p + 1))}
                disabled={!data.hasNextPage}
                className="px-4 py-2 text-sm font-bold rounded-lg border border-gray-200 dark:border-gray-700 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-900 dark:text-white transition-colors"
              >
                التالي
              </button>
            </div>
          )}
        </div>

      </div>
    </main>
  );
};
