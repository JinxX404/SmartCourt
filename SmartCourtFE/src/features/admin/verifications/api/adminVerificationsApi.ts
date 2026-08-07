import { apiClient } from "../../../../api/apiClient";

export interface VerificationDocumentDto {
  documentId: string;
  documentType: string;
  status: "Pending" | "Accepted" | "Rejected";
  rejectionReason?: string | null;
  role?: string;
}

export interface VerificationListDto {
  lawyerId: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  pendingDocumentCount: number;
  verifiedDocumentCount: number;
  rejectedDocumentCount: number;
  role?: string;
}

export interface VerificationDetailsDto {
  lawyerId: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  nationalNumber?: string;
  address?: string;
  dateOfBirth?: string;
  accountStatus: string;
  isFullyVerified: boolean;
  role?: string;
  level?: number;
  specializationName?: string;
  yearsOfExperience?: number;
  bio?: string;
  documents: VerificationDocumentDto[];
  modifiedFields: string[];
}

export const AdminVerificationsApi = {
  // Get all pending verification requests
  getPendingVerifications: async () => {
    const response = await apiClient.get("/api/admin/verifications");
    return response.data;
  },

  // Get details for a specific user
  getVerificationDetails: async (lawyerId: string) => {
    const response = await apiClient.get(`/api/admin/verifications/${lawyerId}`);
    return response.data;
  },

  // Approve or Reject a specific document
  reviewDocument: async (documentId: string, decision: "Approve" | "Reject", rejectionReason?: string) => {
    // Map string decision to C# enum integer (1 = Approve, 2 = Reject)
    const decisionInt = decision === "Approve" ? 1 : 2;
    const response = await apiClient.patch(`/api/admin/verifications/documents/${documentId}`, {
      decision: decisionInt,
      rejectionReason
    });
    return response.data;
  },

  // Helper to generate the URL for the document image
  getDocumentImageUrl: (documentId: string) => {
    const baseUrl = import.meta.env.DEV ? '' : 'http://localhost:5049';
    return `${baseUrl}/api/admin/verifications/documents/${documentId}/content`;
  },

  // Approve entire user account profile
  approveUserAccount: async (userId: string) => {
    const response = await apiClient.patch(`/api/admin/verifications/${userId}/approve-account`);
    return response.data;
  },

  rejectUserAccount: async (userId: string, rejectionReason: string) => {
    const response = await apiClient.patch(`/api/admin/verifications/${userId}/reject-account`, { rejectionReason });
    return response.data;
  }
};
