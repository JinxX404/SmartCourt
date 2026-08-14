import { apiClient } from '../../../api/apiClient';
import type {
  ApiResponse,
  CreateProposalRequest,
  CancelProposalRequest,
  ListProposalsParams,
  ProposalDetailDto,
  ProposalPageDto,
  ProposalSlotAvailabilityDto,
  RejectProposalRequest,
  TerminateProposalRequest,
} from '../types/proposal.types';

export class ProposalApi {
  // ─── List Endpoints ─────────────────────────────────────────────────────────

  /**
   * GET /api/proposals/lawyer
   *
   * Role: Lawyer
   */
  static async getLawyerProposals(
    params: ListProposalsParams = {},
  ): Promise<ApiResponse<ProposalPageDto>> {
    const urlParams = new URLSearchParams();
    if (params.search) urlParams.append('search', params.search);
    if (params.page) urlParams.append('page', params.page.toString());
    if (params.pageSize) urlParams.append('pageSize', params.pageSize.toString());
    
    if (params.statuses && params.statuses.length > 0) {
      params.statuses.forEach(status => urlParams.append('statuses', status));
    }

    const response = await apiClient.get<ApiResponse<ProposalPageDto>>(
      `/api/proposals/lawyer?${urlParams.toString()}`
    );
    return response.data;
  }

  /**
   * GET /api/proposals/cases/{legalCaseId}
   *
   * Role: Client
   */
  static async getCaseProposals(
    legalCaseId: string,
    params: ListProposalsParams = {},
  ): Promise<ApiResponse<ProposalPageDto>> {
    const urlParams = new URLSearchParams();
    if (params.search) urlParams.append('search', params.search);
    if (params.page) urlParams.append('page', params.page.toString());
    if (params.pageSize) urlParams.append('pageSize', params.pageSize.toString());
    
    if (params.statuses && params.statuses.length > 0) {
      params.statuses.forEach(status => urlParams.append('statuses', status));
    }

    const response = await apiClient.get<ApiResponse<ProposalPageDto>>(
      `/api/proposals/cases/${legalCaseId}?${urlParams.toString()}`
    );
    return response.data;
  }

  // ─── Detail & Availability ──────────────────────────────────────────────────

  /**
   * GET /api/proposals/{proposalId}
   *
   * Roles: Client, Lawyer
   */
  static async getProposal(proposalId: string): Promise<ApiResponse<ProposalDetailDto>> {
    const response = await apiClient.get<ApiResponse<ProposalDetailDto>>(
      `/api/proposals/${proposalId}`
    );
    return response.data;
  }

  /**
   * GET /api/proposals/cases/{legalCaseId}/availability
   *
   * Role: Client
   */
  static async checkAvailability(
    legalCaseId: string,
  ): Promise<ApiResponse<ProposalSlotAvailabilityDto>> {
    const response = await apiClient.get<ApiResponse<ProposalSlotAvailabilityDto>>(
      `/api/proposals/cases/${legalCaseId}/availability`
    );
    return response.data;
  }

  // ─── Mutations ──────────────────────────────────────────────────────────────

  /**
   * POST /api/proposals
   *
   * Role: Client
   */
  static async createProposal(
    body: CreateProposalRequest,
  ): Promise<ApiResponse<ProposalDetailDto>> {
    const response = await apiClient.post<ApiResponse<ProposalDetailDto>>(
      '/api/proposals',
      body
    );
    return response.data;
  }

  /**
   * POST /api/proposals/{proposalId}/accept
   *
   * Role: Lawyer
   */
  static async acceptProposal(proposalId: string): Promise<ApiResponse<ProposalDetailDto>> {
    const response = await apiClient.post<ApiResponse<ProposalDetailDto>>(
      `/api/proposals/${proposalId}/accept`
    );
    return response.data;
  }

  /**
   * POST /api/proposals/{proposalId}/reject
   *
   * Role: Lawyer
   */
  static async rejectProposal(
    proposalId: string,
    body: RejectProposalRequest,
  ): Promise<ApiResponse<ProposalDetailDto>> {
    const response = await apiClient.post<ApiResponse<ProposalDetailDto>>(
      `/api/proposals/${proposalId}/reject`,
      body
    );
    return response.data;
  }

  /**
   * POST /api/proposals/{proposalId}/cancel
   *
   * Role: Client
   */
  static async cancelProposal(
    proposalId: string,
    body: CancelProposalRequest,
  ): Promise<ApiResponse<ProposalDetailDto>> {
    const response = await apiClient.post<ApiResponse<ProposalDetailDto>>(
      `/api/proposals/${proposalId}/cancel`,
      body
    );
    return response.data;
  }

  /**
   * POST /api/proposals/{proposalId}/terminate
   *
   * Roles: Client, Lawyer
   */
  static async terminateProposal(
    proposalId: string,
    body: TerminateProposalRequest,
  ): Promise<ApiResponse<ProposalDetailDto>> {
    const response = await apiClient.post<ApiResponse<ProposalDetailDto>>(
      `/api/proposals/${proposalId}/terminate`,
      body
    );
    return response.data;
  }
}
