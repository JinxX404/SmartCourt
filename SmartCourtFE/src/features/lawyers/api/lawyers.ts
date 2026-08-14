import { apiClient } from "../../../api/apiClient";
import type { SearchLawyersRequest, PagedResponse, LawyerSearchDto, LawyerPublicProfileDto, ApiResponse } from "../types";

export const searchLawyers = async (
  params: SearchLawyersRequest
): Promise<PagedResponse<LawyerSearchDto>> => {
  const { data } = await apiClient.get<PagedResponse<LawyerSearchDto>>("/api/lawyers/search", {
    params,
  });
  return data;
};

export const getLawyerProfile = async (
  id: string
): Promise<LawyerPublicProfileDto> => {
  const { data } = await apiClient.get<ApiResponse<LawyerPublicProfileDto>>(`/api/lawyers/public/${id}`);
  if (!data.success) {
    throw new Error(data.message || "Failed to fetch lawyer profile");
  }
  return data.data;
};
