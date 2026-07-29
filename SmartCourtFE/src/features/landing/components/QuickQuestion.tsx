import { RiChatAi3Line } from "react-icons/ri";

export const QuickQuestion = () => {
  return (
    <section className="w-full bg-bg-primary py-16 px-6 flex justify-center transition-colors duration-300">
      
      <div className="w-full max-w-7xl bg-bg-secondary border border-border-primary rounded-[24px] p-8 md:p-12 flex flex-col md:flex-row items-center justify-between gap-8 shadow-card hover:shadow-premium transition-all duration-300">
        
        <div className="flex flex-col items-start text-right gap-3 w-full md:w-auto">
          <span className="text-gold font-bold text-xs uppercase tracking-widest">المساعد الذكي</span>
          <h3 className="text-text-primary text-2xl md:text-3xl font-extrabold leading-tight">
            لديك سؤال قانوني سريع؟
          </h3>
          <p className="text-text-secondary text-sm md:text-base max-w-xl leading-relaxed">
            تحدث مباشرة مع مستشارك القانوني الشخصي المجهز بالذكاء الاصطناعي، واحصل على إجابة موثوقة وفورية في ثوانٍ.
          </p>
        </div>

        <div className="flex shrink-0 w-full md:w-auto justify-end">
          <button className="flex items-center gap-2 px-8 py-3.5 bg-gold hover:bg-gold-hover text-white font-bold text-base rounded-xl transition-all duration-200 hover:scale-[1.02] shadow-xs cursor-pointer">
            <RiChatAi3Line className="w-5 h-5" />
            <span>جرب المستشار الذكي</span>
          </button>
        </div>

      </div>
    </section>
  );
};