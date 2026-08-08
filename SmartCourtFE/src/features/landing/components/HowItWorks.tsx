import { LuMessageSquare, LuBrain, LuUsers, LuShieldCheck } from "react-icons/lu";
import { motion } from "framer-motion";

const STEPS = [
  {
    icon: LuMessageSquare,
    title: "اطرح استفسارك",
    description: "اكتب تفاصيل مشكلتك القانونية أو سؤالك عبر منصتنا بسهولة ووضوح.",
  },
  {
    icon: LuBrain,
    title: "تحليل ذكي وفوري",
    description: "يقوم الذكاء الاصطناعي بتحليل حالتك وتقديم توجيه قانوني مبدئي في ثوانٍ.",
  },
  {
    icon: LuUsers,
    title: "تواصل مع خبير",
    description: "اختر من بين نخبة المحامين المعتمدين لمتابعة قضيتك وتوكيلهم رسمياً.",
  },
  {
    icon: LuShieldCheck,
    title: "ضمان حقوقك",
    description: "أنجز أعمالك القانونية بأمان تام مع تشفير كامل لبياناتك ومستنداتك.",
  },
];

export const HowItWorks = () => {
  return (
    <section id="how-it-works" className="w-full bg-bg-primary py-24 transition-colors duration-300">
      <div className="w-full max-w-4xl mx-auto px-6 flex flex-col gap-16">
        
        {/* Header */}
        <div className="flex flex-col items-center gap-3 text-center">
          <span className="text-gold font-bold text-sm tracking-widest uppercase">طريقة العمل</span>
          <h2 className="text-text-primary text-3xl md:text-4xl font-extrabold tracking-tight">
            كيف يعمل مستشار؟
          </h2>
          <p className="text-text-secondary text-sm md:text-base max-w-xl leading-relaxed">
            أربع خطوات بسيطة ومباشرة تفصلك عن الحصول على أفضل توجيه ودعم قانوني مدعوم بالذكاء الاصطناعي.
          </p>
        </div>

        {/* Vertical Timeline Design */}
        <div className="relative flex flex-col gap-12 w-full mt-4">
          
          {/* Vertical line indicator */}
          <div className="absolute right-6 md:right-1/2 md:translate-x-1/2 top-4 bottom-4 w-[2px] bg-gold/20 -z-10"></div>

          {STEPS.map((step, index) => {
            const Icon = step.icon;
            const isEven = index % 2 === 0;

            return (
              <motion.div 
                key={index}
                initial={{ opacity: 0, y: 50 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true, margin: "-100px" }}
                transition={{ duration: 0.6, delay: index * 0.2, type: "spring", stiffness: 50 }}
                className={`flex flex-col md:flex-row items-start justify-between w-full relative ${
                  isEven ? "md:flex-row-reverse" : ""
                }`}
              >
                
                {/* Timeline dot / icon wrapper */}
                <div className="absolute right-0.5 md:right-1/2 md:translate-x-1/2 w-11 h-11 bg-bg-secondary border-2 border-gold rounded-full flex items-center justify-center text-gold z-10 shadow-xs shrink-0">
                  <Icon className="w-5 h-5" />
                </div>

                {/* Soft Card Content */}
                <div 
                  className={`w-full md:w-[45%] pr-14 md:pr-0 ${
                    isEven ? "md:text-right" : "md:text-left"
                  }`}
                >
                  <div className="p-6 bg-bg-secondary border border-border-primary rounded-xl shadow-card hover:shadow-sm transition-all duration-300">
                    <span className="text-gold/60 font-bold text-xs block mb-1">الخطوة {index + 1}</span>
                    <h3 className="text-text-primary font-bold text-lg leading-relaxed mb-2 text-right">
                      {step.title}
                    </h3>
                    <p className="text-text-secondary text-sm leading-relaxed text-right">
                      {step.description}
                    </p>
                  </div>
                </div>

                {/* Empty spacer for desktop layout */}
                <div className="hidden md:block w-[45%]"></div>

              </motion.div>
            );
          })}

        </div>

      </div>
    </section>
  );
};