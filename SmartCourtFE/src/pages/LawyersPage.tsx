import React, { useState } from "react";
import { LuSearch, LuFilter, LuSlidersHorizontal, LuScale } from "react-icons/lu";
import { useSearchLawyers } from "../features/lawyers/hooks/useSearchLawyers";
import { LawyerCard } from "../features/lawyers/components/LawyerCard";

export const LawyersPage = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [searchInput, setSearchInput] = useState("");
  
  // Example filters (could be expanded)
  const [level, setLevel] = useState<number | undefined>(undefined);
  const [specialization, setSpecialization] = useState<number | undefined>(undefined);
  const [pageNumber, setPageNumber] = useState(1);

  const { data, isLoading, isError } = useSearchLawyers({
    searchTerm,
    level,
    specialization,
    pageNumber,
    pageSize: 12,
  });

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchTerm(searchInput);
    setPageNumber(1); // Reset to page 1 on new search
  };

  return (
    <div className="space-y-6">
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
        <div className="bg-white dark:bg-navy border border-gray-200 dark:border-border-primary rounded-2xl p-4 shadow-sm">
          <form onSubmit={handleSearch} className="flex flex-col md:flex-row gap-4">
            
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

            <div className="flex gap-4">
              <select 
                className="bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white rounded-xl py-3 px-4 focus:outline-none focus:border-gold"
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

              <select 
                className="bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white rounded-xl py-3 px-4 focus:outline-none focus:border-gold"
                value={specialization || ""}
                onChange={(e) => {
                  setSpecialization(e.target.value ? Number(e.target.value) : undefined);
                  setPageNumber(1);
                }}
              >
                <option value="">جميع التخصصات</option>
                <option value="1">جنائي</option>
                <option value="2">مدني</option>
                <option value="3">أسرة</option>
                <option value="4">شركات</option>
                <option value="5">عقاري</option>
                <option value="6">إداري</option>
                <option value="7">جرائم إلكترونية</option>
              </select>

              <button 
                type="submit"
                className="bg-gold text-white font-bold px-6 py-3 rounded-xl hover:bg-gold-hover transition-colors flex items-center gap-2 shadow-sm"
              >
                <LuFilter className="w-5 h-5" />
                بحث
              </button>
            </div>
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
              {data.data.map((lawyer) => (
                <LawyerCard key={lawyer.id} lawyer={lawyer} />
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
    </div>
  );
};
