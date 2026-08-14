import { useQuery } from "@tanstack/react-query";
import type { UseQueryOptions } from "@tanstack/react-query";
import { getLawyerProfile } from "../api/lawyers";
import type { LawyerPublicProfileDto } from "../types";

export const useLawyerProfile = (
  id: string,
  options?: Omit<UseQueryOptions<LawyerPublicProfileDto, Error, LawyerPublicProfileDto>, 'queryKey' | 'queryFn'>
) => {
  return useQuery<LawyerPublicProfileDto, Error>({
    queryKey: ["lawyerProfile", id],
    queryFn: () => getLawyerProfile(id),
    enabled: !!id,
    ...options,
  });
};
