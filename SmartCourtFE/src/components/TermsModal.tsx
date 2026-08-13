import React from "react";
import { LuX, LuCheck } from "react-icons/lu";

interface TermsModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAccept: () => void;
}

export const TermsModal: React.FC<TermsModalProps> = ({ isOpen, onClose, onAccept }) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-navy/80 backdrop-blur-sm transition-opacity">
      <div 
        className="bg-white dark:bg-navy border border-gray-200 dark:border-gray-800 rounded-2xl w-full max-w-3xl max-h-[90vh] flex flex-col shadow-2xl animate-in fade-in zoom-in duration-200"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/50 rounded-t-2xl">
          <h2 className="text-2xl font-bold text-navy dark:text-white">الشروط والأحكام وسياسة الخصوصية</h2>
          <button 
            onClick={onClose}
            className="text-gray-400 hover:text-red-500 transition-colors p-2 rounded-full hover:bg-gray-200 dark:hover:bg-gray-800 bg-white dark:bg-navy shadow-sm"
          >
            <LuX className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="p-8 overflow-y-auto flex-1 space-y-8 text-gray-700 dark:text-gray-300 text-sm leading-loose custom-scrollbar bg-white dark:bg-navy">
          
          <div className="bg-gold/10 p-5 rounded-xl border border-gold/20 text-navy dark:text-white text-base font-medium">
            يرجى قراءة هذه الشروط والأحكام بعناية قبل استخدام منصة "مستشار". استخدامك للمنصة يعني موافقتك التامة على جميع البنود الواردة أدناه.
          </div>

          <section>
            <h3 className="text-xl font-bold text-gold mb-3 flex items-center gap-2">
              <span className="bg-gold text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">1</span>
              مقدمة والتعريفات
            </h3>
            <p className="text-justify">
              أهلاً بكم في منصة "مستشار" الرقمية للخدمات القانونية. تحكم هذه الشروط والأحكام استخدامك للمنصة سواء كنت متصفحاً، أو عميلاً باحثاً عن استشارة، أو محامياً مقدماً للخدمات القانونية. تشكل هذه الشروط عقداً ملزماً قانوناً بينك وبين إدارة المنصة.
            </p>
          </section>

          <section>
            <h3 className="text-xl font-bold text-gold mb-3 flex items-center gap-2">
              <span className="bg-gold text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">2</span>
              شروط إنشاء الحساب والتسجيل
            </h3>
            <ul className="list-disc list-inside space-y-2 pr-2">
              <li>يجب ألا يقل عمر المستخدم عن 18 عاماً لإنشاء حساب في المنصة.</li>
              <li>تتعهد بأن جميع المعلومات التي تقدمها أثناء التسجيل (كالاسم، رقم الهوية، ورقم الهاتف) هي معلومات صحيحة، دقيقة، وحديثة.</li>
              <li>بالنسبة للمحامين: يشترط تقديم وثائق تثبت القيد في نقابة المحامين وأن تكون سارية المفعول، ولا يحق للمحامي تقديم أي خدمة إلا بعد اجتياز مرحلة "التوثيق" من قبل إدارة المنصة.</li>
              <li>أنت المسؤول الوحيد عن الحفاظ على سرية بيانات الدخول الخاصة بحسابك، وتتحمل المسؤولية الكاملة عن أي نشاط يتم عبر حسابك.</li>
            </ul>
          </section>

          <section>
            <h3 className="text-xl font-bold text-gold mb-3 flex items-center gap-2">
              <span className="bg-gold text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">3</span>
              التزامات المحامي
            </h3>
            <ul className="list-disc list-inside space-y-2 pr-2">
              <li>الالتزام بميثاق شرف مهنة المحاماة وتقديم الاستشارات والخدمات القانونية بأعلى درجات المهنية والأمانة.</li>
              <li>عدم تقديم أي وعود أو ضمانات قاطعة بنتيجة القضايا بما يخالف طبيعة العمل القانوني الذي يعتمد على بذل العناية وليس تحقيق الغاية.</li>
              <li>الحفاظ التام على سرية بيانات العملاء وتفاصيل قضاياهم وعدم الإفصاح عنها لأي طرف ثالث.</li>
              <li>تحديث الحالة المهنية فوراً في حال الإيقاف عن العمل أو شطب القيد من النقابة.</li>
            </ul>
          </section>

          <section>
            <h3 className="text-xl font-bold text-gold mb-3 flex items-center gap-2">
              <span className="bg-gold text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">4</span>
              التزامات العميل
            </h3>
            <ul className="list-disc list-inside space-y-2 pr-2">
              <li>تقديم معلومات ووثائق صحيحة للمحامي لتمكينه من دراسة القضية وتقديم الرأي القانوني السليم.</li>
              <li>الالتزام بدفع الرسوم والأتعاب المتفق عليها من خلال قنوات الدفع الرسمية المتاحة داخل المنصة.</li>
              <li>عدم استخدام المنصة لأي أغراض غير قانونية، أو احتيالية، أو لرفع قضايا كيدية.</li>
            </ul>
          </section>

          <section>
            <h3 className="text-xl font-bold text-gold mb-3 flex items-center gap-2">
              <span className="bg-gold text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">5</span>
              الرسوم والمدفوعات (نظام الضمان)
            </h3>
            <p className="text-justify mb-2">
              لحماية حقوق الطرفين، تعتمد المنصة نظام الحساب الوسيط (Escrow):
            </p>
            <ul className="list-disc list-inside space-y-2 pr-2">
              <li>يقوم العميل بسداد قيمة الخدمة أو الاستشارة مسبقاً، وتبقى الأموال معلقة في حساب المنصة.</li>
              <li>لا يتم تحويل المبلغ إلى المحامي إلا بعد تقديم الخدمة المتفق عليها وإغلاق الطلب بنجاح.</li>
              <li>تستقطع المنصة عمولة إدارية (موضحة في صفحة الدفع) لقاء تقديم خدمات الربط والدعم الفني.</li>
              <li>في حال نشوب نزاع بين المحامي والعميل، يحق لإدارة المنصة التدخل ومراجعة المحادثات واتخاذ القرار برد المبلغ للعميل أو تحويله للمحامي.</li>
            </ul>
          </section>

          <section>
            <h3 className="text-xl font-bold text-gold mb-3 flex items-center gap-2">
              <span className="bg-gold text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">6</span>
              سياسة الخصوصية وحماية البيانات
            </h3>
            <p className="text-justify mb-2">
              خصوصيتك هي أولويتنا القصوى:
            </p>
            <ul className="list-disc list-inside space-y-2 pr-2">
              <li>نقوم بجمع بياناتك الأساسية (الاسم، البريد الإلكتروني، رقم الهاتف، والوثائق الشخصية) لأغراض التوثيق وتشغيل المنصة فقط.</li>
              <li>جميع المحادثات والملفات المتبادلة بين العميل والمحامي مشفرة ومحمية ولا يحق لأي طرف الإطلاع عليها إلا في حالات النزاعات الرسمية.</li>
              <li>لن نقوم أبداً ببيع، أو تأجير، أو مشاركة بياناتك الشخصية مع جهات تسويقية خارجية.</li>
            </ul>
          </section>
          
          <section>
            <h3 className="text-xl font-bold text-gold mb-3 flex items-center gap-2">
              <span className="bg-gold text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">7</span>
              إخلاء المسؤولية
            </h3>
            <p className="text-justify">
              تعمل منصة "مستشار" كوسيط تكنولوجي يربط بين العملاء والمحامين المرخصين. الآراء، الاستشارات، والمذكرات القانونية المقدمة تعبر بشكل كامل وحصري عن رأي المحامي المستقل الذي أعدها. ولا تتحمل المنصة، أو ملاكها، أو موظفوها أي مسؤولية قانونية، مدنية أو جنائية، ناتجة عن دقة، أو صحة، أو فعالية الاستشارات المقدمة.
            </p>
          </section>

          <section>
            <h3 className="text-xl font-bold text-gold mb-3 flex items-center gap-2">
              <span className="bg-gold text-white w-8 h-8 rounded-full flex items-center justify-center text-sm">8</span>
              إنهاء الحساب
            </h3>
            <p className="text-justify">
              تحتفظ إدارة المنصة بالحق المطلق في إيقاف أو إلغاء حساب أي مستخدم (عميل أو محامي) فوراً ودون إنذار مسبق في حال اكتشاف تلاعب، تقديم أوراق مزورة، إساءة استخدام المنصة، أو انتهاك أي من هذه الشروط والأحكام.
            </p>
          </section>

        </div>

        {/* Footer */}
        <div className="p-6 border-t border-gray-100 dark:border-gray-800 flex justify-end gap-3 bg-gray-50 dark:bg-gray-900/50 rounded-b-2xl">
          <button
            onClick={onClose}
            className="px-6 py-2.5 rounded-xl font-bold text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-800 transition-colors"
          >
            إغلاق
          </button>
          <button
            onClick={() => {
              onAccept();
              onClose();
            }}
            className="px-6 py-2.5 rounded-xl font-bold text-white bg-gold hover:bg-gold-hover transition-colors flex items-center gap-2 shadow-premium"
          >
            <LuCheck className="w-5 h-5" />
            أوافق على الشروط
          </button>
        </div>

      </div>
    </div>
  );
};
