import { HeroSection,TrustBar,QuickQuestion,FeaturedLawyers,HowItWorks } from "../features/landing";

export const Home = () => {
  return (
    <main className="flex flex-col min-h-screen w-full">

      <HeroSection />
      <TrustBar/>
      <HowItWorks/>
      <FeaturedLawyers/>
      <QuickQuestion/>
    </main>
  );
};