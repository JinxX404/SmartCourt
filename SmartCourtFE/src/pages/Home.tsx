import {
  HeroSection,
  StatisticsSection,
  HowItWorks,
  FeaturedLawyers,
  TestimonialsSection,
  QuickQuestion
} from "../features/landing";

export const Home = () => {
  return (
    <main className="flex flex-col min-h-screen w-full">
      <HeroSection />
      <HowItWorks />
      <StatisticsSection />
      <FeaturedLawyers />
      <TestimonialsSection />
      <QuickQuestion />
    </main>
  );
};