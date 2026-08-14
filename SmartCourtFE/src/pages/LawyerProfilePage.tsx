import React from "react";
import { useParams, useNavigate } from "react-router-dom";
import { LuArrowRight, LuStar, LuMapPin, LuAward, LuShieldCheck } from "react-icons/lu";
import { useLawyerProfile } from "../features/lawyers/hooks/useLawyerProfile";
import { getLevelName, getSpecializationName } from "../features/lawyers/utils";
import { Loader } from "../components/Loader";

export const LawyerProfilePage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: lawyer, isLoading, isError, error } = useLawyerProfile(id || "");

  if (isLoading) {
    return (
      <div className="flex-1 flex justify-center items-center h-full min-h-[60vh]">
        <Loader />
      </div>
    );
  }

  if (isError || !lawyer) {
    return (
      <div className="flex-1 flex flex-col justify-center items-center h-full min-h-[60vh] text-center">
        <h2 className="text-2xl font-bold text-gray-800 dark:text-white mb-2">حدث خطأ</h2>
        <p className="text-gray-500 mb-6">{error?.message || "لم نتمكن من العثور على الملف الشخصي للمحامي."}</p>
        <button 
          onClick={() => navigate(-1)}
          className="bg-gold text-white px-6 py-2 rounded-xl hover:bg-gold-hover transition-colors"
        >
          العودة للبحث
        </button>
      </div>
    );
  }

  return (
    <main className="flex-1 p-4 sm:p-8 overflow-y-auto w-full bg-gray-50/50 dark:bg-transparent">
      <div className="max-w-4xl mx-auto space-y-6">
        
        {/* Navigation / Header */}
        <button 
          onClick={() => navigate(-1)}
          className="flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-gold transition-colors font-medium"
        >
          <LuArrowRight className="w-5 h-5" />
          العودة للنتائج
        </button>

        {/* Profile Card */}
        <div className="bg-white dark:bg-navy border border-gray-200 dark:border-border-primary rounded-2xl overflow-hidden shadow-sm relative">
          
          {/* Cover */}
          <div className="h-32 bg-gradient-to-r from-gold/20 to-gold/5 dark:from-gold/10 dark:to-transparent" />
          
          <div className="px-6 pb-6 sm:px-10 sm:pb-10 -mt-16">
            <div className="flex flex-col sm:flex-row gap-6 items-start sm:items-end">
              
              {/* Avatar */}
              <div className="relative">
                {lawyer.profilePictureUrl ? (
                  <img 
                    src={lawyer.profilePictureUrl} 
                    alt={lawyer.name}
                    className="w-32 h-32 rounded-2xl object-cover border-4 border-white dark:border-navy shadow-md bg-white dark:bg-navy"
                  />
                ) : (
                  <div className="w-32 h-32 rounded-2xl bg-gradient-to-br from-gold to-yellow-600 flex items-center justify-center border-4 border-white dark:border-navy shadow-md text-white font-bold text-4xl">
                    {lawyer.name.charAt(0)}
                  </div>
                )}
                {lawyer.isAvailable && (
                  <span className="absolute bottom-2 right-2 w-4 h-4 bg-green-500 border-2 border-white dark:border-navy rounded-full" title="متاح حالياً"></span>
                )}
              </div>

              {/* Main Info */}
              <div className="flex-1 space-y-2">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                  <div>
                    <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
                      {lawyer.name}
                      <LuShieldCheck className="w-6 h-6 text-blue-500" title="موثق" />
                    </h1>
                    <p className="flex items-center gap-1.5 text-gray-500 dark:text-gray-400 mt-1 font-medium">
                      <LuShieldCheck className="w-4 h-4 text-gold" />
                      محامي {getLevelName(lawyer.level)}
                    </p>
                  </div>

                  <div className="flex gap-3">
                    <button className="bg-gold text-white px-6 py-2.5 rounded-xl font-bold hover:bg-gold-hover transition-colors shadow-sm whitespace-nowrap">
                      طلب استشارة
                    </button>
                    <button className="border border-gold text-gold px-6 py-2.5 rounded-xl font-bold hover:bg-gold/5 transition-colors whitespace-nowrap">
                      توكيل
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Content Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          
          {/* Main Content */}
          <div className="md:col-span-2 space-y-6">
            
            {/* Bio */}
            <div className="bg-white dark:bg-navy border border-gray-200 dark:border-border-primary rounded-2xl p-6 sm:p-8 shadow-sm">
              <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">نبذة تعريفية</h2>
              <p className="text-gray-600 dark:text-gray-300 leading-relaxed whitespace-pre-wrap">
                {lawyer.bio || "لا توجد نبذة تعريفية مضافة لهذا المحامي."}
              </p>
            </div>

            {/* Specializations */}
            <div className="bg-white dark:bg-navy border border-gray-200 dark:border-border-primary rounded-2xl p-6 sm:p-8 shadow-sm">
              <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <LuAward className="w-5 h-5 text-gold" />
                التخصصات والخبرات
              </h2>
              
              {lawyer.specializations && lawyer.specializations.length > 0 ? (
                <div className="space-y-4">
                  {lawyer.specializations.map((spec, index) => (
                    <div key={index} className="flex flex-col sm:flex-row sm:items-center justify-between p-4 bg-gray-50 dark:bg-gray-800/50 rounded-xl border border-gray-100 dark:border-gray-700">
                      <div className="flex items-center gap-3 mb-2 sm:mb-0">
                        <div className="w-10 h-10 bg-gold/10 rounded-lg flex items-center justify-center text-gold">
                          <LuAward className="w-5 h-5" />
                        </div>
                        <div>
                          <h3 className="font-bold text-gray-900 dark:text-white">{getSpecializationName(spec.specialization)}</h3>
                          <p className="text-sm text-gray-500">خبرة {spec.yearsOfExperience} سنوات</p>
                        </div>
                      </div>
                      <div className="text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 px-4 py-2 rounded-lg shadow-sm border border-gray-200 dark:border-gray-600">
                        {spec.casesHandled} قضية مكتملة
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-gray-500">لم يقم المحامي بإضافة تخصصاته بعد.</p>
              )}
            </div>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            
            {/* Quick Stats */}
            <div className="bg-white dark:bg-navy border border-gray-200 dark:border-border-primary rounded-2xl p-6 shadow-sm">
              <h3 className="font-bold text-gray-900 dark:text-white mb-4">معلومات سريعة</h3>
              <div className="space-y-4">
                
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg flex items-center justify-center text-yellow-500">
                    <LuStar className="w-5 h-5 fill-current" />
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">التقييم العام</p>
                    <p className="font-bold text-gray-900 dark:text-white">{lawyer.rating > 0 ? lawyer.rating.toFixed(1) : "مستخدم جديد"}</p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 bg-blue-50 dark:bg-blue-900/20 rounded-lg flex items-center justify-center text-blue-500">
                    <LuMapPin className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">الموقع</p>
                    <p className="font-bold text-gray-900 dark:text-white">{lawyer.governorate || "لم يحدد"}</p>
                  </div>
                </div>

              </div>
            </div>
            
          </div>
        </div>

      </div>
    </main>
  );
};
