import React from 'react';
import { MdSearch, MdFilterList } from 'react-icons/md';

export const CasesHeader: React.FC = () => {
  return (
    <div className="mb-8 flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 mb-1">إدارة القضايا</h1>
        <p className="text-gray-500 text-sm">
          نظرة شاملة ومتابعة دقيقة لجميع ملفاتك القانونية.
        </p>
      </div>
      <div className="flex items-center gap-4 self-start md:self-auto">
        <div className="relative hidden md:block">
          <MdSearch className="absolute right-3 top-2.5 text-gray-400 text-xl" />
          <input 
            type="text"
            className="pr-10 pl-4 bg-white border border-gray-300 rounded-lg py-2 text-sm w-72 focus:ring-2 focus:ring-gray-200 focus:border-gray-400 text-gray-900 transition-colors" 
            placeholder="ابحث برقم القضية أو العنوان..." 
          />
        </div>
        <button className="bg-white border border-gray-300 text-gray-700 px-4 py-2 rounded-lg font-medium text-sm flex items-center gap-2 hover:bg-gray-50 transition-colors">
          تصفية
          <MdFilterList className="text-xl" />
        </button>
      </div>
    </div>
  );
};
