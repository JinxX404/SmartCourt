import { apiClient } from '../../../api/apiClient';
import type { ApiResponse, CaseListItemDto, CaseReviewReportDto } from '../types/case.types';

export interface CaseDocumentDto {
  id: string;
  fileName: string | null;
  fileUrl: string | null;
  contentType: string | null;
}

export interface CaseDto {
  id: string;
  clientId: string;
  title: string | null;
  description: string | null;
  governorate: string | null;
  city: string | null;
  status: string | null;
  createdAt: string;
  documents: CaseDocumentDto[] | null;
}

export interface CreateCaseRequest {
  title: string;
  description: string;
  governorate: string;
  city: string;
  documents: File[];
}

export class CasesApi {
  /**
   * Creates a new case with documents using multipart/form-data.
   */
  static async createCase(data: CreateCaseRequest): Promise<CaseDto> {
    const formData = new FormData();
    formData.append('title', data.title);
    formData.append('description', data.description);
    formData.append('governorate', data.governorate);
    formData.append('city', data.city);

    if (data.documents && data.documents.length > 0) {
      data.documents.forEach((file) => {
        formData.append(`documents`, file);
      });
    }

    const response = await apiClient.post('/api/Case', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  /**
   * Gets all cases.
   */
  static async getCases(): Promise<CaseDto[]> {
    const response = await apiClient.get('/api/Case');
    return response.data;
  }

  /**
   * Gets a specific case by id.
   */
  static async getCaseById(id: string): Promise<CaseDto> {
    const response = await apiClient.get(`/api/Case/${id}`);
    return response.data;
  }

  /**
   * Updates a specific case.
   */
  static async updateCase(id: string, data: Partial<CreateCaseRequest>): Promise<CaseDto> {
    const formData = new FormData();
    if (data.title) formData.append('title', data.title);
    if (data.description) formData.append('description', data.description);
    if (data.governorate) formData.append('governorate', data.governorate);
    if (data.city) formData.append('city', data.city);
    
    if (data.documents && data.documents.length > 0) {
      data.documents.forEach((file) => {
        formData.append(`documents`, file);
      });
    }

    const response = await apiClient.put(`/api/Case/${id}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  /**
   * Deletes a specific case.
   */
  static async deleteCase(id: string): Promise<void> {
    const response = await apiClient.delete(`/api/Case/${id}`);
    return response.data;
  }

  /**
   * Finalizes a specific case.
   */
  static async finalizeCase(id: string): Promise<void> {
    const response = await apiClient.post(`/api/Case/${id}/finalize`);
    return response.data;
  }

  /**
   * Gets the list of cases for the dashboard.
   */
  static async fetchCasesList(): Promise<ApiResponse<CaseListItemDto[]>> {
    const response = await apiClient.get<ApiResponse<CaseListItemDto[]>>('/api/Case');
    return response.data;
  }

  /**
   * Triggers an AI review for a case.
   */
  static async triggerAiReview(id: string): Promise<void> {
    await apiClient.post(`/api/cases/${id}/review`);
  }

  /**
   * Gets the latest review for a case.
   */
  static async getLatestReview(id: string): Promise<ApiResponse<CaseReviewReportDto>> {
    const response = await apiClient.get<ApiResponse<CaseReviewReportDto>>(`/api/cases/${id}/reviews/latest`);
    return response.data;
  }

  /**
   * Gets the lawyer recommendations for a case.
   */
  static async getRecommendations(id: string): Promise<any> {
    const response = await apiClient.get(`/api/cases/${id}/recommendations`);
    return response.data;
  }

  /**
   * Downloads a document for a case.
   */
  static async downloadDocument(caseId: string, documentId: string): Promise<{ data: Blob, contentType: string, fileName: string }> {
    const response = await apiClient.get(`/api/Case/${caseId}/documents/${documentId}/download`, {
      responseType: 'blob'
    });
    
    // Extract file name from Content-Disposition header if possible
    let fileName = 'document';
    const contentDisposition = response.headers['content-disposition'];
    if (contentDisposition && contentDisposition.indexOf('filename=') !== -1) {
      const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(contentDisposition);
      if (matches != null && matches[1]) {
        fileName = matches[1].replace(/['"]/g, '');
      }
    }
    
    return {
      data: response.data,
      contentType: response.headers['content-type'] as string,
      fileName: fileName
    };
  }
}
