import { apiClient } from "../../../api/apiClient";
import type { SearchLawyersRequest, PagedResponse, LawyerSearchDto } from "../types";

export const searchLawyers = async (
  params: SearchLawyersRequest
): Promise<PagedResponse<LawyerSearchDto>> => {
  const { data } = await apiClient.get<PagedResponse<LawyerSearchDto>>("/api/lawyers/search", {
    params,
  });
  return data;
};
