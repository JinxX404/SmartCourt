import { motion, useSpring, useTransform } from 'framer-motion';
import { useEffect } from 'react';
import { useInView } from 'react-intersection-observer';

const STATS = [
  {
    prefix: "+",
    value: 10,
    suffix: " آلاف",
    label: "استشارة قانونية",
    description: "تمت بنجاح وموثوقية عبر المنصة",
  },
  {
    prefix: "",
    value: 98,
    suffix: "٪",
    label: "دقة الإجابات",
    description: "بشهادة مراجعينا وخبرائنا القانونيين",
  },
  {
    prefix: "",
    value: 24,
    suffix: "/7",
    label: "متاح دائماً",
    description: "دعم مستمر وإجابات ذكية في أي وقت",
  },
];

const AnimatedCounter = ({ to, inView }: { to: number; inView: boolean }) => {
  const spring = useSpring(0, { bounce: 0, duration: 2500 });
  const display = useTransform(spring, (current) => Math.round(current).toString());

  useEffect(() => {
    if (inView) {
      spring.set(to);
    }
  }, [spring, to, inView]);

  return <motion.span>{display}</motion.span>;
};

export const StatisticsSection = () => {
  const { ref, inView } = useInView({
    triggerOnce: true,
    threshold: 0.2,
  });

  return (
    <section ref={ref} className="w-full bg-bg-secondary border-y border-border-primary py-16 transition-colors duration-300">
      <div className="w-full max-w-7xl mx-auto px-6">
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-12 md:gap-0 items-center justify-between">
          {STATS.map((stat, index) => (
            <div 
              key={index}
              className={`flex flex-col items-center justify-center text-center gap-2 px-6 ${
                index !== STATS.length - 1 ? "md:border-l md:border-border-primary" : ""
              }`}
            >
              {/* Stat Value */}
              <span className="text-gold font-extrabold text-5xl md:text-6xl tracking-tight leading-none" dir="ltr">
                {stat.prefix}
                {inView ? (
                  <AnimatedCounter to={stat.value} inView={inView} />
                ) : (
                  "0"
                )}
                {stat.suffix}
              </span>
              
              {/* Stat Label */}
              <span className="text-text-primary font-bold text-lg leading-tight mt-1.5">
                {stat.label}
              </span>
              
              {/* Stat Description */}
              <span className="text-text-secondary text-sm max-w-[200px] leading-relaxed">
                {stat.description}
              </span>
            </div>
          ))}
        </div>

      </div>
    </section>
  );
};
