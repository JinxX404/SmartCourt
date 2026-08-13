import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
    LuLayoutDashboard,
    LuFolder,
    LuUsers,
    LuShieldCheck,
    LuMessageSquare,
    LuSettings,
    LuCircleHelp,
    LuLogOut,
    LuPlus,
    LuUser,
    LuInbox
} from "react-icons/lu";

import { NotificationBell } from "../features/notifications/components/NotificationBell";
import { FaChevronLeft, FaChevronRight } from "react-icons/fa6";
import { BiSolidHome } from "react-icons/bi";

export interface SidebarProps {
    user: any;
    activeTab: string;
    handleTabChange: (tab: string) => void;
    sidebarOpen: boolean;
    setSidebarOpen: (open: boolean) => void;
    handleLogout: () => void;
    getRoleLabel: (role?: string) => string;
    profilePictureUrl: string | null;
}

export const Sidebar = ({
    user,
    activeTab,
    handleTabChange,
    sidebarOpen,
    setSidebarOpen,
    handleLogout,
    getRoleLabel,
    profilePictureUrl,
}: SidebarProps) => {
    const navigate = useNavigate();
    const [isCollapsed, setIsCollapsed] = useState(false);

    const getTabStyles = (tabName: string) => {
        return activeTab === tabName
            ? `flex w-full items-center ${isCollapsed ? 'justify-center' : 'gap-4 px-4'} bg-[#c5a059] text-[#121620] rounded-lg py-3 transition-all duration-200 shadow-sm border border-[#c5a059]/30 font-bold`
            : `flex w-full items-center ${isCollapsed ? 'justify-center' : 'gap-4 px-4'} text-gray-400 hover:bg-white/5 hover:text-white py-3 rounded-lg transition-all duration-200`;
    };

    return (
        <nav className={`bg-[#121620] border-l border-gray-800/60 shadow-md flex-col fixed md:sticky right-0 top-0 z-40 md:flex shrink-0 h-screen transition-all duration-300 ${isCollapsed ? "w-20" : "w-64"} ${sidebarOpen ? "translate-x-0" : "translate-x-full md:translate-x-0"}`}>

            {/* Mobile Close Button (Optional, can be added if needed, or handled via header) */}

            <div className={`p-6 border-b border-gray-800/60 flex flex-col items-center relative ${isCollapsed ? 'px-2' : ''}`}>

                {/* Green Box (Home) */}
                {!isCollapsed && (
                    <button onClick={() => navigate("/")} className="absolute top-4 right-4 text-gray-400 hover:text-white transition-colors cursor-pointer" title="الرئيسية">
                        <BiSolidHome className="w-6 h-6" />
                    </button>
                )}

                {/* Red Box (Notification) */}
                <div className={`absolute top-4 left-4 ${isCollapsed ? 'static top-auto left-auto mb-6' : ''}`}>
                    <NotificationBell />
                </div>

                {/* Avatar */}
                <div className={`rounded-full border-2 border-[#c5a059] overflow-hidden shadow-[0_0_15px_rgba(197,160,89,0.3)] bg-[#1a1d23] flex items-center justify-center transition-all ${isCollapsed ? 'w-12 h-12 mb-0' : 'w-20 h-20 mb-4'}`}>
                    {profilePictureUrl ? (
                        <img alt={user?.fullName || "Profile"} className="w-full h-full object-cover" src={profilePictureUrl} />
                    ) : user?.fullName ? (
                        <span className={`text-[#c5a059] font-bold ${isCollapsed ? 'text-xl' : 'text-3xl'}`}>{user.fullName.charAt(0).toUpperCase()}</span>
                    ) : (
                        <LuUser className={`text-[#c5a059] ${isCollapsed ? 'w-6 h-6' : 'w-10 h-10'}`} />
                    )}
                </div>

                {/* Yellow Box (Collapse Toggle) */}
                <button
                    onClick={() => setIsCollapsed(!isCollapsed)}
                    className={`flex items-center justify-center text-gray-400 hover:text-white transition-colors cursor-pointer ${isCollapsed ? 'static w-full h-8 mt-6 rounded' : 'absolute left-4 bottom-8 w-8 h-8 rounded'}`}
                    title={isCollapsed ? "توسيع القائمة" : "طي القائمة"}
                >
                    {isCollapsed ? <FaChevronLeft className="w-4 h-4" /> : <FaChevronRight className="w-4 h-4" />}
                </button>

                {!isCollapsed && (
                    <>
                        <h2 className="text-lg text-white font-bold">{user?.fullName || "مستخدم"}</h2>
                        <p className="text-[#c5a059] text-sm mt-1">{getRoleLabel(user?.role)}</p>
                    </>
                )}
            </div>

            <div className={`p-4 ${isCollapsed ? 'px-2' : ''}`}>
                <button
                    onClick={() => handleTabChange("new-case")}
                    className={`bg-[#c5a059] text-white rounded-lg py-3 flex items-center justify-center gap-2 text-sm font-bold hover:bg-[#b08d4b] transition-colors shadow-lg shadow-[#c5a059]/20 cursor-pointer ${isCollapsed ? 'w-full px-0' : 'w-full'}`}
                    title="طلب استشارة جديدة"
                >
                    <LuPlus className="w-5 h-5 shrink-0" />
                    {!isCollapsed && <span>رفع قضية جديدة</span>}
                </button>
            </div>

            <ul className="flex-1 py-4 space-y-2 overflow-y-auto dark-scrollbar px-2">
                <li>
                    <button onClick={() => handleTabChange("overview")} className={getTabStyles("overview")} title="الرئيسية">
                        <LuLayoutDashboard className="w-5 h-5 shrink-0" />
                        {!isCollapsed && <span className="text-sm">الرئيسية</span>}
                    </button>
                </li>
                <li>
                    <button onClick={() => handleTabChange("cases")} className={getTabStyles("cases")} title="قضاياي">
                        <LuFolder className="w-5 h-5 shrink-0" />
                        {!isCollapsed && <span className="text-sm">قضاياي</span>}
                    </button>
                </li>
                <li>
                    <button onClick={() => { navigate("/dashboard/lawyers"); setSidebarOpen(false); }} className={activeTab === "lawyers" ? getTabStyles("lawyers") : getTabStyles("none")} title="البحث عن محامين">
                        <LuUsers className="w-5 h-5 shrink-0" />
                        {!isCollapsed && <span className="text-sm">البحث عن محامين</span>}
                    </button>
                </li>

                {user?.role !== 'Admin' && (
                    <li>
                        <button onClick={() => handleTabChange("verification")} className={getTabStyles("verification")} title="توثيق الحساب">
                            <LuShieldCheck className="w-5 h-5 shrink-0" />
                            {!isCollapsed && <span className="text-sm">توثيق الحساب</span>}
                        </button>
                    </li>
                )}

                {user?.role === 'Admin' && (
                    <li>
                        <button onClick={() => handleTabChange("admin-verifications")} className={getTabStyles("admin-verifications")} title="إدارة التوثيقات">
                            <LuShieldCheck className="w-5 h-5 shrink-0" />
                            {!isCollapsed && <span className="text-sm">إدارة التوثيقات</span>}
                        </button>
                    </li>
                )}

                {user?.role === 'Lawyer' && (
                    <li>
                        <button onClick={() => handleTabChange("proposals")} className={getTabStyles("proposals")} title="العروض">
                            <LuInbox className="w-5 h-5 shrink-0" />
                            {!isCollapsed && <span className="text-sm">العروض</span>}
                        </button>
                    </li>
                )}

                <li>
                    <button onClick={() => { navigate("/dashboard/chat"); setSidebarOpen(false); }} className={activeTab === "chat" ? getTabStyles("chat") : getTabStyles("none")} title="المحادثات">
                        <LuMessageSquare className="w-5 h-5 shrink-0" />
                        {!isCollapsed && <span className="text-sm">المحادثات</span>}
                    </button>
                </li>
                <li>
                    <button onClick={() => handleTabChange("settings")} className={getTabStyles("settings")} title="الإعدادات">
                        <LuSettings className="w-5 h-5 shrink-0" />
                        {!isCollapsed && <span className="text-sm">الإعدادات</span>}
                    </button>
                </li>
            </ul>

            <div className="border-t border-gray-800/60 p-4 space-y-2 px-2 mt-auto">
                <button className={`flex w-full items-center text-gray-400 hover:bg-white/5 hover:text-white py-3 rounded-lg transition-all duration-200 cursor-pointer ${isCollapsed ? 'justify-center' : 'gap-4 px-4'}`} title="مركز المساعدة">
                    <LuCircleHelp className="w-5 h-5 shrink-0" />
                    {!isCollapsed && <span className="text-sm">مركز المساعدة</span>}
                </button>
                <button onClick={handleLogout} className={`flex w-full items-center text-red-500 hover:bg-red-500/10 py-3 rounded-lg transition-all duration-200 cursor-pointer ${isCollapsed ? 'justify-center' : 'gap-4 px-4'}`} title="تسجيل الخروج">
                    <LuLogOut className="w-5 h-5 shrink-0" />
                    {!isCollapsed && <span className="text-sm">تسجيل الخروج</span>}
                </button>
            </div>
        </nav>
    );
};
