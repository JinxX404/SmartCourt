import { useQuery } from "@tanstack/react-query";
import type { UseQueryOptions } from "@tanstack/react-query";
import { searchLawyers } from "../api/lawyers";
import type { SearchLawyersRequest, PagedResponse, LawyerSearchDto } from "../types";

export const useSearchLawyers = (
  params: SearchLawyersRequest,
  options?: Omit<UseQueryOptions<PagedResponse<LawyerSearchDto>, Error, PagedResponse<LawyerSearchDto>>, 'queryKey' | 'queryFn'>
) => {
  return useQuery<PagedResponse<LawyerSearchDto>, Error>({
    queryKey: ["lawyers", params],
    queryFn: () => searchLawyers(params),
    ...options,
  });
};
