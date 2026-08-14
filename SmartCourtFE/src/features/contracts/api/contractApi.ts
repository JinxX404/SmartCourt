import { apiClient } from '../../../api/apiClient';
import type {
  ApiResponse,
  ContractDetailDto,
  ContractSummaryDto,
  PagedResult,
  CreateContractRequest,
  UpdateContractRequest,
  TerminateContractRequest,
  ContractActionResultDto,
  ContractStateHistoryDto,
  ContractStatus
} from '../types/contract.types';

export const contractApi = {
  /** 
   * GET /api/contracts
   * List the current user's contracts.
   */
  async getContracts(
    params?: { status?: ContractStatus; page?: number; pageSize?: number }
  ): Promise<ApiResponse<PagedResult<ContractSummaryDto>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<ContractSummaryDto>>>('/api/contracts', { params });
    return response.data;
  },

  /** 
   * GET /api/contracts/{contractId}
   * Get Contract detail, returning the snapshot, optimistic concurrency token (version), etc.
   */
  async getContract(contractId: string): Promise<ApiResponse<ContractDetailDto>> {
    const response = await apiClient.get<ApiResponse<ContractDetailDto>>(`/api/contracts/${contractId}`);
    return response.data;
  },

  /**
   * POST /api/contracts
   * The Lawyer creates one Draft contract from an already accepted proposal.
   */
  async createContract(data: CreateContractRequest): Promise<ApiResponse<ContractDetailDto>> {
    const response = await apiClient.post<ApiResponse<ContractDetailDto>>('/api/contracts', data);
    return response.data;
  },

  /**
   * PUT /api/contracts/{contractId}
   * Replaces the Draft title and terms. Only the Contract Lawyer can update it.
   * IMPORTANT: Requires If-Match header using the exact quoted Base64 version.
   */
  async updateContract(
    contractId: string, 
    version: string, 
    data: UpdateContractRequest
  ): Promise<ApiResponse<ContractDetailDto>> {
    const response = await apiClient.put<ApiResponse<ContractDetailDto>>(
      `/api/contracts/${contractId}`, 
      data,
      { headers: { 'If-Match': version } }
    );
    return response.data;
  },

  /**
   * POST /api/contracts/{contractId}/accept
   * Records acceptance by the calling Client or Lawyer.
   * IMPORTANT: Requires If-Match header.
   */
  async acceptContract(contractId: string, version: string): Promise<ApiResponse<ContractActionResultDto>> {
    const response = await apiClient.post<ApiResponse<ContractActionResultDto>>(
      `/api/contracts/${contractId}/accept`,
      {},
      { headers: { 'If-Match': version } }
    );
    return response.data;
  },

  /**
   * POST /api/contracts/{contractId}/terminate
   * A Contract party requests termination.
   * IMPORTANT: Requires If-Match header.
   */
  async terminateContract(
    contractId: string, 
    version: string, 
    data: TerminateContractRequest
  ): Promise<ApiResponse<ContractDetailDto>> {
    const response = await apiClient.post<ApiResponse<ContractDetailDto>>(
      `/api/contracts/${contractId}/terminate`,
      data,
      { headers: { 'If-Match': version } }
    );
    return response.data;
  },

  /**
   * GET /api/contracts/{contractId}/state-history
   * Returns the auditable Contract status-transition history, newest first.
   */
  async getContractHistory(
    contractId: string, 
    params?: { page?: number; pageSize?: number }
  ): Promise<ApiResponse<PagedResult<ContractStateHistoryDto>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<ContractStateHistoryDto>>>(
      `/api/contracts/${contractId}/state-history`,
      { params }
    );
    return response.data;
  },
};
