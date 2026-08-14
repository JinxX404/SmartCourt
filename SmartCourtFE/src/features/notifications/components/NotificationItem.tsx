import { LuInfo, LuX, LuCheck } from "react-icons/lu";
import type { NotificationDto } from "../types";
import { useQuery } from "@tanstack/react-query";
import { AdminVerificationsApi } from "../../admin/verifications/api/adminVerificationsApi";

const getDocumentName = (type?: string | number) => {
  const typeStr = type?.toString();
  switch (typeStr) {
    case "NationalIdFront": 
    case "1": return "بطاقة الرقم القومي (الأمام)";
    case "NationalIdBack": 
    case "2": return "بطاقة الرقم القومي (الخلف)";
    case "BarAssociationCardFront": 
    case "3": return "كارنيه النقابة (الأمام)";
    case "BarAssociationCardBack": 
    case "4": return "كارنيه النقابة (الخلف)";
    case "SelfieWithId": 
    case "5": return "صورة شخصية مع الهوية";
    case "Other": 
    case "6": return "مستند إضافي";
    case "OfficialProfilePicture": 
    case "7": return "الصورة الشخصية الرسمية";
    default: return "مستند تحقق";
  }
};

const formatNotificationTitle = (notification: NotificationDto) => {
  if (notification.type.startsWith("verification.document") && notification.data?.documentType) {
    const docName = getDocumentName(notification.data.documentType);
    if (notification.type === "verification.document-approved") return `تم اعتماد مستند: ${docName}`;
    if (notification.type === "verification.document-rejected") return `تم رفض مستند: ${docName}`;
    if (notification.type === "verification.document-expired") return `انتهت صلاحية مستند: ${docName}`;
  }
  return notification.title;
};

interface NotificationItemProps {
  notification: NotificationDto;
  onClick: (notification: NotificationDto) => void;
}

export function NotificationItem({ notification, onClick }: NotificationItemProps) {
  const isUnread = !notification.readAtUtc;
  const isReviewRequested = notification.type === "verification.review-requested";
  const userId = notification.data?.userId;

  const { data: userDetails } = useQuery({
    queryKey: ["admin", "verifications", userId],
    queryFn: () => AdminVerificationsApi.getVerificationDetails(userId!),
    enabled: isReviewRequested && !!userId,
    staleTime: 5 * 60 * 1000, // cache for 5 minutes
  });

  const getNotificationTitle = () => {
    if (isReviewRequested && userDetails?.data) {
      const roleStr = userDetails.data.role === 'Lawyer' ? 'المحامي' : 'العميل';
      return `طلب مراجعة من ${roleStr}: ${userDetails.data.fullName}`;
    }
    return formatNotificationTitle(notification);
  };

  const getNotificationBody = () => {
    if (isReviewRequested && userDetails?.data) {
      const pendingDocs = userDetails.data.documents?.filter((d: any) => d.status === "Pending") || [];
      if (pendingDocs.length > 0) {
        const docNames = pendingDocs.map((d: any) => getDocumentName(d.documentType)).join('، ');
        return `المستندات المرفوعة: ${docNames}.`;
      }
    }
    return notification.body;
  };

  const getIcon = () => {
    switch (notification.severity) {
      case "Info":
        return <LuInfo className="w-5 h-5 text-blue-500" />;
      case "Warning":
        return <LuInfo className="w-5 h-5 text-yellow-500" />;
      case "Error":
        return <LuX className="w-5 h-5 text-red-500" />;
      case "Success":
        return <LuCheck className="w-5 h-5 text-green-500" />;
      default:
        return <LuInfo className="w-5 h-5 text-gray-500" />;
    }
  };

  const getBgColor = () => {
    if (!isUnread) return "bg-white dark:bg-gray-800";
    return "bg-blue-50 dark:bg-blue-900/20";
  };

  return (
    <button
      onClick={() => onClick(notification)}
      className={`w-full text-right p-4 border-b border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors flex items-start gap-3 ${getBgColor()}`}
    >
      <div className="flex-shrink-0 mt-1">{getIcon()}</div>
      <div className="flex-1 space-y-1">
        <div className="flex items-center justify-between">
          <p className={`text-sm ${isUnread ? "font-bold text-gray-900 dark:text-white" : "font-semibold text-gray-700 dark:text-gray-300"}`}>
            {getNotificationTitle()}
          </p>
          <span className="text-xs text-gray-500 dark:text-gray-400">
            {new Date(notification.createdAtUtc).toLocaleDateString("ar-EG")}
          </span>
        </div>
        <p className={`text-sm line-clamp-2 leading-relaxed ${isUnread ? "text-gray-700 dark:text-gray-300" : "text-gray-500 dark:text-gray-400"}`}>
          {getNotificationBody()}
        </p>
      </div>
      {isUnread && (
        <div className="flex-shrink-0 w-2 h-2 rounded-full bg-blue-500 mt-2"></div>
      )}
    </button>
  );
}
