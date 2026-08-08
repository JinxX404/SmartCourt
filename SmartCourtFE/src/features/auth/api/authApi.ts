import { apiClient } from '../../../api/apiClient';


export interface LoginRequest {
  email: string;
  password?: string;
}



export interface RegisterClientRequest {
  fullName: string;
  email: string;
  password?: string;
  confirmPassword?: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword?: string;
  confirmNewPassword?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface RevokeTokenRequest {
  token: string;
  refreshToken: string;
}

export interface SubmitVerificationRequest {
  userId: string;
  documents: {
    file: File;
    expirationDate: string;
    type: number;
  }[];
}

export class AuthApi {
  /**
   * Authenticates a user and returns tokens.
   */
  static async login(data: LoginRequest) {
    const response = await apiClient.post('/api/auth/login', data);
    return response.data;
  }



  /**
   * Registers a new unverified client account.
   */
  static async registerClient(data: RegisterClientRequest) {
    const response = await apiClient.post('/api/auth/register/client', data);
    return response.data;
  }

  /**
   * Registers a new unverified lawyer account.
   */
  static async registerLawyer(data: RegisterClientRequest) {
    const response = await apiClient.post('/api/auth/register/lawyer', data);
    return response.data;
  }

  /**
   * Rotates active refresh token and returns new token pair.
   */
  static async refresh(refreshToken?: string) {
    const response = await apiClient.post('/api/auth/refresh', refreshToken ? { refreshToken } : {});
    return response.data;
  }

  /**
   * Revokes supplied refresh token.
   */
  static async revoke(data: RevokeTokenRequest) {
    const response = await apiClient.post('/api/auth/revoke', data);
    return response.data;
  }

  /**
   * Changes authenticated user's password.
   */
  static async changePassword(data: ChangePasswordRequest) {
    const response = await apiClient.post('/api/auth/change-password', data);
    return response.data;
  }

  /**
   * Confirms email address from route tokens.
   */
  static async confirmEmail(userId: string, token: string) {
    const response = await apiClient.get('/api/auth/confirm-email', {
      params: { userId, token },
    });
    return response.data;
  }

  /**
   * Sends password-reset request link.
   */
  static async forgotPassword(email: string) {
    const response = await apiClient.post('/api/auth/forgot-password', { email });
    return response.data;
  }

  /**
   * Resets password using token.
   */
  static async resetPassword(data: ResetPasswordRequest) {
    const response = await apiClient.post('/api/auth/reset-password', data);
    return response.data;
  }

  /**
   * Resends email confirmation code.
   */
  static async resendVerification(email: string) {
    const response = await apiClient.post('/api/auth/resend-verification', { email });
    return response.data;
  }

  /**
   * Submits user verification documents.
   */
  static async submitVerificationDocuments(data: SubmitVerificationRequest) {
    const formData = new FormData();
    formData.append('UserId', data.userId);
    
    data.documents.forEach((doc, index) => {
      formData.append(`Documents[${index}].File`, doc.file);
      formData.append(`Documents[${index}].ExpirationDate`, doc.expirationDate);
      formData.append(`Documents[${index}].Type`, doc.type.toString());
    });

    const response = await apiClient.post('/api/UserVerification/submit-verification-documents', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      }
    });
    return response.data;
  }

  /**
   * Gets user verification documents.
   */
  static async getUserVerificationDocuments(userId: string) {
    const response = await apiClient.get(`/api/UserVerification/${userId}`);
    return response.data;
  }

  /**
   * Helper to generate the URL for the user's document image
   */
  static getDocumentImageUrl(documentId: string) {
    const baseUrl = import.meta.env.DEV ? '' : 'http://localhost:5049';
    return `${baseUrl}/api/UserVerification/documents/${documentId}/content`;
  }

  /**
   * Gets document content details (including downloadUrl)
   */
  static async getDocumentContent(documentId: string) {
    const response = await apiClient.get(`/api/UserVerification/documents/${documentId}/content`);
    return response.data;
  }

  /**
   * Sends phone verification OTP.
   */
  static async sendPhoneVerificationToken(phoneNumber: string) {
    const response = await apiClient.post('/api/auth/phone/send-token', { phoneNumber });
    return response.data;
  }

  /**
   * Confirms phone verification OTP.
   */
  static async confirmPhoneVerification(phoneNumber: string, token: string) {
    const response = await apiClient.post('/api/auth/phone/confirm', { phoneNumber, token });
    return response.data;
  }
}
