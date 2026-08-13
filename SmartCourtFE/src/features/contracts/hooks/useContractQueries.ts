import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useEffect } from 'react';
import toast from 'react-hot-toast';
import { contractApi } from '../api/contractApi';
import { notificationsHub } from '../../notifications/api/notificationsHub';
import type { 
  ContractStatus, 
  CreateContractRequest, 
  UpdateContractRequest, 
  TerminateContractRequest 
} from '../types/contract.types';

const CONTRACT_QUERY_KEYS = {
  all: ['contracts'] as const,
  lists: () => [...CONTRACT_QUERY_KEYS.all, 'list'] as const,
  list: (filters: { status?: ContractStatus; page?: number; pageSize?: number }) => 
    [...CONTRACT_QUERY_KEYS.lists(), filters] as const,
  details: () => [...CONTRACT_QUERY_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...CONTRACT_QUERY_KEYS.details(), id] as const,
  histories: () => [...CONTRACT_QUERY_KEYS.all, 'history'] as const,
  history: (id: string) => [...CONTRACT_QUERY_KEYS.histories(), id] as const,
};

// ----------------------------------------
// QUERIES
// ----------------------------------------

export function useContracts(params?: { status?: ContractStatus; page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: CONTRACT_QUERY_KEYS.list(params || {}),
    queryFn: () => contractApi.getContracts(params),
  });
}

export function useContract(contractId: string) {
  return useQuery({
    queryKey: CONTRACT_QUERY_KEYS.detail(contractId),
    queryFn: () => contractApi.getContract(contractId),
    enabled: !!contractId,
  });
}

export function useContractHistory(contractId: string, params?: { page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: CONTRACT_QUERY_KEYS.history(contractId),
    queryFn: () => contractApi.getContractHistory(contractId, params),
    enabled: !!contractId,
  });
}

// ----------------------------------------
// MUTATIONS
// ----------------------------------------

export function useCreateContract() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateContractRequest) => contractApi.createContract(data),
    onSuccess: (response) => {
      if (response.data) {
        queryClient.invalidateQueries({ queryKey: CONTRACT_QUERY_KEYS.lists() });
        queryClient.setQueryData(CONTRACT_QUERY_KEYS.detail(response.data.id), response);
        toast.success('Contract created successfully');
      }
    },
    onError: (error: any) => {
      // Typically intercept custom error envelope
      const msg = error?.response?.data?.message || 'Failed to create contract';
      toast.error(msg);
    }
  });
}

export function useUpdateContractDraft(contractId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ version, data }: { version: string, data: UpdateContractRequest }) => 
      contractApi.updateContract(contractId, version, data),
    onSuccess: (response) => {
      // Invalidate and set new cache data seamlessly
      queryClient.setQueryData(CONTRACT_QUERY_KEYS.detail(contractId), response);
      toast.success('Contract draft updated');
    },
    onError: (error: any) => {
      if (error?.response?.status === 409) {
        toast.error('The contract was modified by another process. Refreshing...');
        // Force refresh to get latest version string
        queryClient.invalidateQueries({ queryKey: CONTRACT_QUERY_KEYS.detail(contractId) });
      } else {
        const msg = error?.response?.data?.message || 'Failed to update draft';
        toast.error(msg);
      }
    }
  });
}

export function useAcceptContract(contractId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ version }: { version: string }) => contractApi.acceptContract(contractId, version),
    onSuccess: () => {
      // The API POST /accept does NOT return the new version token.
      // We MUST explicitly invalidate to fetch the fresh token immediately.
      queryClient.invalidateQueries({ queryKey: CONTRACT_QUERY_KEYS.detail(contractId) });
      toast.success('Contract accepted');
    },
    onError: (error: any) => {
      if (error?.response?.status === 409) {
        toast.error('Version conflict or you already accepted. Refreshing...');
        queryClient.invalidateQueries({ queryKey: CONTRACT_QUERY_KEYS.detail(contractId) });
      } else {
        const msg = error?.response?.data?.message || 'Failed to accept contract';
        toast.error(msg);
      }
    }
  });
}

export function useTerminateContract(contractId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ version, data }: { version: string, data: TerminateContractRequest }) => 
      contractApi.terminateContract(contractId, version, data),
    onSuccess: (response) => {
      queryClient.setQueryData(CONTRACT_QUERY_KEYS.detail(contractId), response);
      toast.success('Contract termination requested');
    },
    onError: (error: any) => {
      if (error?.response?.status === 409) {
        toast.error('Settlement conflict or pending settlement. Please check back shortly.');
        // Background process might be settling, refresh the current state.
        queryClient.invalidateQueries({ queryKey: CONTRACT_QUERY_KEYS.detail(contractId) });
      } else {
        const msg = error?.response?.data?.message || 'Failed to terminate contract';
        toast.error(msg);
      }
    }
  });
}

// ----------------------------------------
// SIGNAL-R NOTIFICATIONS SYNC
// ----------------------------------------

/**
 * Hook to automatically keep the contract UI synchronized with the backend.
 * Place this in a high-level ContractView component.
 */
export function useContractNotificationsSync(contractId?: string) {
  const queryClient = useQueryClient();

  useEffect(() => {
    // We only care about events if we have a contract to sync (or general list sync)
    
    // Using a generic handler for all notifications
    const handleNotification = (notification: any) => {
      // We need to check if the notification pertains to our contractId or contracts in general
      const eventType = notification?.type; // assuming the payload has a type like 'contract.draft-updated'
      const relatedId = notification?.entityId || notification?.contractId; // varies by backend payload structure

      const contractEvents = [
        'contract.draft-updated',
        'contract.acceptance-recorded',
        'contract.activated',
        'contract.completed',
        'contract.termination-requested',
        'contract.terminated'
      ];

      if (contractEvents.includes(eventType)) {
        if (contractId && relatedId === contractId) {
          // A change happened to the contract we are viewing. 
          // Aggressively refetch to get the latest `version` and status.
          queryClient.invalidateQueries({ queryKey: CONTRACT_QUERY_KEYS.detail(contractId) });
          queryClient.invalidateQueries({ queryKey: CONTRACT_QUERY_KEYS.history(contractId) });
        } else if (!contractId) {
          // We might be on the list view. Refetch lists.
          queryClient.invalidateQueries({ queryKey: CONTRACT_QUERY_KEYS.lists() });
        }
      }
    };

    // Attach to hub
    const unsubscribe = notificationsHub.onNotificationCreated(handleNotification);

    return () => {
      unsubscribe();
    };
  }, [contractId, queryClient]);
}
