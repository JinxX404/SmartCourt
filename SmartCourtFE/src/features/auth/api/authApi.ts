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
}
