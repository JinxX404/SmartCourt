import { RiChatAi3Line } from "react-icons/ri";

export const QuickQuestion = () => {
  return (
    <section className="w-full bg-white py-12 px-6 flex justify-center">
      
      <div className="w-full max-w-7xl bg-navy border border-secondary-gray rounded-2xl p-8 md:p-10 flex flex-col md:flex-row items-center justify-between gap-6 md:gap-0">
        
        <div className="flex flex-col items-start text-right gap-2 w-full md:w-auto">
          <h3 className="text-white text-2xl font-bold">
            لديك سؤال قانوني سريع؟
          </h3>
          <p className="text-gray-400 text-base">
            احصل على مساعدة ذكية قانونية فورية وموثوقة وقتما تحتاجها
          </p>
        </div>

        <div className="flex flex-row items-center gap-4 w-full md:w-auto justify-end">
          <button className="flex gap-2 bg-gold-muted hover:bg-gold-muted-hover text-white font-bold text-base px-8 py-3 rounded-lg transition-colors duration-200 shrink-0">
             <RiChatAi3Line className="w-6 h-6" />
            جرب الآن
          </button>
        </div>

      </div>
    </section>
  );
};