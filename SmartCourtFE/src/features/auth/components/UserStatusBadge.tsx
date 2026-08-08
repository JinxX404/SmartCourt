import {
  LuShieldCheck,
  LuTriangleAlert,
  LuClock,
  LuBan
} from "react-icons/lu";

export const UserStatusBadge = ({ status, role }: { status?: string, role?: string }) => {
  if (role && role !== 'Client' && role !== 'Lawyer') return null;

  switch (status) {
    case 'Active':
      return (
        <span className="inline-flex items-center w-fit gap-1 text-[10px] text-green-500 font-bold bg-green-500/10 px-2 py-0.5 rounded-full border border-green-500/20 whitespace-nowrap">
          <LuShieldCheck className="w-3 h-3" />
          موثق
        </span>
      );
    case 'Unverified':
      return (
        <span className="inline-flex items-center w-fit gap-1 text-[10px] text-red-500 font-bold bg-red-500/10 px-2 py-0.5 rounded-full border border-red-500/20 whitespace-nowrap">
          <LuTriangleAlert className="w-3 h-3" />
          غير موثق
        </span>
      );
    case 'PendingReview':
      return (
        <span className="inline-flex items-center w-fit gap-1 text-[10px] text-amber-500 font-bold bg-amber-500/10 px-2 py-0.5 rounded-full border border-amber-500/20 whitespace-nowrap">
          <LuClock className="w-3 h-3" />
          قيد المراجعة
        </span>
      );
    case 'Rejected':
      return (
        <span className="inline-flex items-center w-fit gap-1 text-[10px] text-amber-500 font-bold bg-amber-500/10 px-2 py-0.5 rounded-full border border-amber-500/20 whitespace-nowrap">
          <LuTriangleAlert className="w-3 h-3" />
          بانتظار إعادة التوثيق
        </span>
      );
    case 'Suspended':
      return (
        <span className="inline-flex items-center w-fit gap-1 text-[10px] text-gray-500 font-bold bg-gray-500/10 px-2 py-0.5 rounded-full border border-gray-500/20 whitespace-nowrap">
          <LuBan className="w-3 h-3" />
          موقوف
        </span>
      );
    default:
      return null;
  }
};
