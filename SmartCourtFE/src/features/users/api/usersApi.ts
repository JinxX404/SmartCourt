import { apiClient } from '../../../api/apiClient';

export interface ClientProfile {
  id: string;
  name: string;
  email: string;
  phoneNumber?: string;
  nationalNumber?: string;
  gender?: string;
  dateOfBirth?: string;
  address?: string;
  governorate?: string;
  city?: string;
  status: string;
}

export interface UpdateClientProfileRequest {
  nationalNumber?: string;
  dateOfBirth?: string;
  address?: string;
  governorate?: string;
  city?: string;
  gender?: number;
}

export interface LawyerProfile {
  id: string;
  name: string;
  email: string;
  phoneNumber?: string;
  nationalNumber?: string;
  gender?: string;
  dateOfBirth?: string;
  specializations?: Array<{
    specialization: number;
    specializationName: string;
    yearsOfExperience: number;
    casesHandled: number;
  }>;
  level?: number; // 1=GeneralRegistration, 2=PrimaryCourt, 3=AppealCourt, 4=CassationCourt
  bio?: string;
  address?: string;
  governorate?: string;
  city?: string;
  status: string;
  isAvailable?: boolean;
  profilePictureUrl?: string;
}

export interface UpdateLawyerProfileRequest {
  nationalNumber?: string;
  dateOfBirth?: string;
  gender?: number; // 0=Male, 1=Female
  level?: number;
  bio?: string;
  address?: string;
  governorate?: string;
  city?: string;
  specializations?: Array<{
    specialization: number;
    yearsOfExperience: number;
    casesHandled: number;
  }>;
}

export interface DeleteProfileRequest {
  currentPassword: string;
}

export class UsersApi {
  /**
   * Retrieves the profile of the authenticated client.
   */
  static async getClientProfile(): Promise<ClientProfile> {
    const response = await apiClient.get('/api/clients/profile');
    return response.data?.data || response.data;
  }

  /**
   * Updates the authenticated client's profile.
   */
  static async updateClientProfile(data: UpdateClientProfileRequest) {
    const response = await apiClient.put('/api/clients/profile', data);
    return response.data;
  }

  /**
   * Soft-deletes the authenticated client account.
   */
  static async deleteClientProfile(data: DeleteProfileRequest) {
    const response = await apiClient.delete('/api/clients/profile', { data });
    return response.data;
  }

  /**
   * Retrieves the authenticated lawyer's private profile.
   */
  static async getLawyerProfile(): Promise<LawyerProfile> {
    const response = await apiClient.get('/api/lawyers/profile');
    return response.data?.data || response.data;
  }

  /**
   * Updates the authenticated lawyer's profile.
   */
  static async updateLawyerProfile(data: UpdateLawyerProfileRequest) {
    const response = await apiClient.put('/api/lawyers/profile', data);
    return response.data;
  }

  /**
   * Soft-deletes the authenticated lawyer account.
   */
  static async deleteLawyerProfile(data: DeleteProfileRequest) {
    const response = await apiClient.delete('/api/lawyers/profile', { data });
    return response.data;
  }

  /**
   * Retrieves a public lawyer profile.
   */
  static async getPublicLawyerProfile(id: string) {
    const response = await apiClient.get(`/api/lawyers/public/${id}`);
    return response.data?.data || response.data;
  }
}
