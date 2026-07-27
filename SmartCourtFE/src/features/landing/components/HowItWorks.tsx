const WORKFLOW_STEPS = [
  {
    title: "اطرح استفسارك",
    description: "اكتب تفاصيل مشكلتك القانونية أو سؤالك عبر منصتنا بسهولة ووضوح.",
  },
  {
    title: "تحليل ذكي وفوري",
    description: "يقوم الذكاء الاصطناعي بتحليل حالتك وتقديم توجيه قانوني مبدئي في ثوانٍ.",
  },
  {
    title: "تواصل مع خبير",
    description: "اختر من بين نخبة المحامين المعتمدين لمتابعة قضيتك وتوكيلهم رسمياً.",
  },
  {
    title: "ضمان حقوقك",
    description: "أنجز أعمالك القانونية بأمان تام مع تشفير كامل لبياناتك ومستنداتك.",
  }
];

export const HowItWorks = () => {
  return (
    <section className="w-full bg-navy py-24 flex flex-col items-center relative z-10">
      
      {/* max-w-[1280px] is exactly max-w-7xl in Tailwind */}
      <div className="w-full max-w-7xl px-6 flex flex-col items-center gap-16">
        
        <div className="flex flex-col items-center gap-4">
          <h2 className="text-white text-3xl font-bold text-center">
            كيف يعمل مستشار؟
          </h2>
          <div className="w-12 h-1 bg-gold rounded-full"></div>
        </div>

        {/* max-w-[1024px] is exactly max-w-5xl */}
        <div className="relative w-full max-w-5xl">
          
          <div className="hidden md:block absolute h-0.5 left-0 right-0 top-6 bg-gold/30 -z-10"></div>

          <div className="flex flex-col md:flex-row justify-between items-center md:items-start gap-12 md:gap-4 w-full">
            
            {WORKFLOW_STEPS.map((step, index) => (
              <div 
                key={index} 
                className="flex flex-col items-center flex-1 w-full md:max-w-58"
              >
                <div className="w-12 h-12 bg-navy border-4 border-gold rounded-full flex items-center justify-center mb-4">
                  <span className="text-white font-bold text-lg">
                    {index + 1}
                  </span>
                </div>

                <h4 className="text-white font-bold text-base mb-2 text-center">
                  {step.title}
                </h4>
                {/* #9ca3af is gray-400 */}
                <p className="text-gray-400 text-sm md:text-xs text-center leading-relaxed max-w-55">
                  {step.description}
                </p>
              </div>
            ))}

          </div>
        </div>
      </div>
    </section>
  );
};