import { useState, useEffect } from "react";
import { Outlet, useNavigate, useLocation, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "../features/auth/store/useAuthStore";
import { AuthApi } from "../features/auth/api/authApi";
import { Sidebar } from "./Sidebar";
import { UserStatusBadge } from "../features/auth/components/UserStatusBadge";
import { useNotifications } from "../features/notifications/hooks/useNotifications";
import { LuScale, LuMenu, LuX } from "react-icons/lu";

export const DashboardLayout = () => {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();

  // Determine active tab based on route or query params
  const activeTab = location.pathname.includes("/dashboard/chat")
    ? "chat"
    : location.pathname.includes("/dashboard/lawyers")
    ? "lawyers"
    : searchParams.get("tab") || "cases";

  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Initialize SignalR notifications
  useNotifications();

  const { data: documentsData } = useQuery({
    queryKey: ["user", "verifications", "documents", user?.id],
    queryFn: () => AuthApi.getUserVerificationDocuments(user!.id),
    enabled: !!user?.id && (user?.role === "Lawyer" || user?.role === "Client"),
  });

  // profileData removed since it's unused

  // targetProgress removed because it's unused in this layout

  const profilePictureDoc = documentsData?.data?.documents?.find(
    (d: any) => (d.documentType === "OfficialProfilePicture" || d.documentType === 7) && d.isCurrent
  );
  const isPictureApproved = profilePictureDoc?.status === "Verified" || profilePictureDoc?.status === 2;

  const { data: profilePicContent } = useQuery({
    queryKey: ["documentContent", profilePictureDoc?.documentId],
    queryFn: () => AuthApi.getDocumentContent(profilePictureDoc!.documentId),
    enabled: !!profilePictureDoc?.documentId && isPictureApproved,
  });

  const profilePictureUrl = isPictureApproved ? profilePicContent?.data?.downloadUrl || null : null;

  useEffect(() => {
    if (!user) {
      navigate("/login");
    }
  }, [user, navigate]);

  const handleTabChange = (tab: string) => {
    if (tab === "chat") {
      navigate("/dashboard/chat");
    } else {
      navigate(`/dashboard?tab=${tab}`);
    }
    setSidebarOpen(false);
  };

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const getRoleLabel = (role?: string) => {
    switch (role) {
      case "Lawyer":
        return "محامي ";
      case "Client":
        return "موكل";
      case "Admin":
        return "مسؤول منصة";
      default:
        return "مستخدم";
    }
  };

  if (!user) return null;

  return (
    <div className="h-[100dvh] md:h-screen overflow-hidden bg-[#f4f5f8] dark:bg-[#0d1017] text-text-primary flex flex-col md:flex-row transition-colors duration-300">
      
      {/* Mobile Header Bar */}
      <div className="md:hidden flex items-center justify-between p-4 bg-[#121620] text-white sticky top-0 z-40 border-b border-gray-800">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-full bg-gold/20 text-gold flex items-center justify-center border border-gold/40 font-bold overflow-hidden">
            {profilePictureUrl ? (
              <img src={profilePictureUrl} alt={user?.fullName || "Profile"} className="w-full h-full object-cover" />
            ) : user?.fullName ? (
              user.fullName.charAt(0).toUpperCase()
            ) : (
              <LuScale className="w-5 h-5" />
            )}
          </div>
          <div>
            <div className="flex items-center gap-2">
              <p className="text-sm font-bold text-white">{user?.fullName}</p>
              <UserStatusBadge status={user?.status} role={user?.role} />
            </div>
            <p className="text-[10px] text-gold font-bold">{getRoleLabel(user?.role)}</p>
          </div>
        </div>
        <div className="flex items-center gap-1">
          <button onClick={() => setSidebarOpen(!sidebarOpen)} className="p-2 rounded-xl bg-gray-800 text-white">
            {sidebarOpen ? <LuX className="w-6 h-6" /> : <LuMenu className="w-6 h-6" />}
          </button>
        </div>
      </div>

      {/* RIGHT SIDEBAR */}
      <Sidebar
        user={user}
        activeTab={activeTab}
        handleTabChange={handleTabChange}
        sidebarOpen={sidebarOpen}
        setSidebarOpen={setSidebarOpen}
        handleLogout={handleLogout}
        getRoleLabel={getRoleLabel}
        profilePictureUrl={profilePictureUrl}
      />

      {/* MAIN CONTENT AREA */}
      <Outlet context={{ activeTab, user }} />
    </div>
  );
};
