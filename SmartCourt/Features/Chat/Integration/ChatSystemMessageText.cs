namespace SmartCourt.Features.Chat.Integration;

internal static class ChatSystemMessageText
{
    public static string For(ContractConversationMessageType type)
    {
        return type switch
        {
            ContractConversationMessageType.ContractCreated =>
                "تم إنشاء مسودة العقد.",
            ContractConversationMessageType.ContractAccepted =>
                "تمت الموافقة على مسودة العقد.",
            ContractConversationMessageType.ContractActivated =>
                "العقد أصبح نشطاً وسارياً الآن.",
            ContractConversationMessageType.ContractCompleted =>
                "تم اكتمال العقد بنجاح.",
            ContractConversationMessageType.MilestoneReadyForFunding =>
                "المرحلة جاهزة للتمويل وتأكيد الدفعة.",
            ContractConversationMessageType.MilestoneFundingStarted =>
                "بدأت عملية تمويل المرحلة.",
            ContractConversationMessageType.MilestoneFunded =>
                "تم تمويل المرحلة وإيداع المبلغ في الضمان.",
            ContractConversationMessageType.MilestoneFundingFailed =>
                "فشلت عملية تمويل المرحلة.",
            ContractConversationMessageType.MilestoneSubmitted =>
                "تم تسليم أعمال المرحلة للمراجعة والاعتماد.",
            ContractConversationMessageType.MilestoneAutoAccepted =>
                "تم قبول المرحلة واعتمادها تلقائياً.",
            ContractConversationMessageType.MilestoneAccepted =>
                "تم قبول المرحلة واعتماد الأعمال.",
            ContractConversationMessageType.MilestoneChangesRequested =>
                "تم طلب تعديلات على أعمال المرحلة.",
            ContractConversationMessageType.MilestoneChangeRequestApproved =>
                "تمت الموافقة على طلب تعديل المرحلة.",
            ContractConversationMessageType.MilestoneChangeRequestRejected =>
                "تم رفض طلب تعديل المرحلة.",
            ContractConversationMessageType.MilestoneChangeRequestCancelled =>
                "تم إلغاء طلب تعديل المرحلة.",
            ContractConversationMessageType.DisputeOpened =>
                "تم فتح نزاع بشأن العقد.",
            ContractConversationMessageType.DisputeAssigned =>
                "تم تعيين وسيط / محكم للنزاع.",
            ContractConversationMessageType.DisputeResolved =>
                "تم حل النزاع والتوصل لاتفاق.",
            ContractConversationMessageType.DisputeClosed =>
                "تم إغلاق ملف النزاع.",
            ContractConversationMessageType.FundsReleased =>
                "تم تحرير الدفعة المالية للمحامي.",
            ContractConversationMessageType.FundsRefunded =>
                "تم استرداد الدفعة المالية للموكل.",
            ContractConversationMessageType.ContractTerminated =>
                "تم إنهاء العقد وتصفيته.",
            _ => "تم تحديث المحادثة والاتفاق."
        };
    }
}
