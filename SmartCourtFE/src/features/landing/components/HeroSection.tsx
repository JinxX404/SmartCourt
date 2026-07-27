import { LuScale, LuFileText, LuCheck, LuShield } from "react-icons/lu";

export const HeroSection = () => {
  return (
    <section className="relative w-full bg-linear-to-b from-gradient-start to-navy pt-20 pb-40 flex flex-col items-center">
      
      <div className="flex flex-col items-center gap-6 w-full max-w-7xl px-6">
        
        <div className="relative flex flex-col items-center w-full max-w-308">
          <h1 className="text-white text-5xl font-bold leading-tight text-center relative z-10">
            مرحباً بك في المنصة القانونية
            <br />
            <span className="relative inline-block mt-2">
              الذكية والأولى
              <span className="absolute -bottom-3 left-0 right-0 h-0.75 bg-linear-to-r from-transparent via-gold to-transparent rounded-sm"></span>
            </span>
          </h1>
          
          {/* #9ca3af is gray-400, max-w-[672px] is max-w-2xl */}
          <p className="text-gray-400 text-lg leading-relaxed text-center max-w-2xl mt-12">
            نوفر لك استشارات قانونية دقيقة وسريعة مدعومة بأحدث تقنيات الذكاء الاصطناعي لضمان حقوقك وتسهيل إجراءاتك القانونية بكل موثوقية.
          </p>
        </div>

        {/* max-w-[896px] is max-w-4xl */}
        <div className="flex flex-col md:flex-row justify-center items-start pt-10 gap-8 w-full max-w-4xl">
          
          {/* Replaced arbitrary shadow with shadow-premium */}
          <div className="relative flex flex-row items-start p-8 bg-white rounded-xl shadow-premium w-full md:w-108 min-h-36.25">
            <div className="absolute -top-4 -right-4 w-12 h-12 bg-gold border-4 border-navy rounded-full flex items-center justify-center z-10">
               <LuCheck className="text-white w-5 h-5 border border-white rounded-full p-0.5" />
            </div>

            <div className="flex flex-row items-start gap-4 w-full">
              <div className="flex flex-col items-start gap-2 flex-1">
                <h3 className="text-navy text-xl font-bold leading-relaxed text-start">
                  استشارة فورية
                </h3>
                {/* #6b7280 is gray-500 */}
                <p className="text-gray-500 text-sm leading-relaxed text-start">
                  احصل على توجيه قانوني سريع ودقيق لمعاملاتك من خلال الذكاء الاصطناعي.
                </p>
              </div>

              {/* #f3f4f6 is gray-100 */}
              <div className="flex flex-col items-center justify-center w-14 h-14 bg-gray-100 rounded-lg border-[2.6px] border-navy shrink-0">
                <LuScale className="w-8 h-8 text-navy" />
              </div>
            </div>
          </div>

          <div className="relative flex flex-row items-start p-8 bg-white rounded-xl shadow-premium w-full md:w-108 min-h-36.25">
            <div className="absolute -top-4 -right-4 w-12 h-12 bg-gold border-4 border-navy rounded-full flex items-center justify-center z-10">
               <LuShield className="text-white w-5 h-5 border border-white rounded-full p-0.5" />
            </div>

            <div className="flex flex-row items-start gap-4 w-full">
              <div className="flex flex-col items-start gap-2 flex-1">
                <h3 className="text-navy text-xl font-bold leading-relaxed text-start">
                  صياغة العقود
                </h3>
                <p className="text-gray-500 text-sm leading-relaxed text-start">
                  أدوات احترافية لصياغة ومراجعة العقود القانونية بما يتوافق مع الأنظمة.
                </p>
              </div>

              <div className="flex flex-col items-center justify-center w-14 h-14 bg-gray-100 rounded-lg border-[2.6px] border-navy shrink-0">
                <LuFileText className="w-8 h-8 text-navy" />
              </div>
            </div>
          </div>

        </div>
      </div>
    </section>
  );
};