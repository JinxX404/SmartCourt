import React from 'react';
import { LuArrowRight } from 'react-icons/lu';
import { CreateCaseForm } from './CreateCaseForm';

interface CreateCaseTabProps {
  onBack?: () => void;
}

export const CreateCaseTab: React.FC<CreateCaseTabProps> = ({ onBack }) => {
  return (
    <div className="animate-fade-in w-full max-w-5xl mx-auto py-2">
      {/* Page Header */}
      <div className="mb-10 flex items-center gap-4">

        {onBack && (
          <button 
            onClick={onBack}
            className="w-10 h-10 flex items-center justify-center rounded-full bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 text-gray-900 dark:text-white hover:border-gold transition-colors"
          >
            <LuArrowRight className="w-5 h-5" />
          </button>
        )}

        
        <div>
          <h1 className="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white">رفع قضية جديدة</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-2">يرجى تعبئة النموذج أدناه بدقة لضمان مراجعة فعالة لحالتك القانونية.</p>
        </div>
      </div>

      {/* Form Component */}
      <CreateCaseForm />
      
      {/* Safe Space Bottom */}
      <div className="h-10"></div>
    </div>
  );
};
