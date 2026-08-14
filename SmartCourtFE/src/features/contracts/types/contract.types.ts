export const ContractStatus = {
  Draft: 0,
  Active: 1,
  SuspendedByDispute: 2,
  Completed: 3,
  Terminated: 4,
} as const;
export type ContractStatus = typeof ContractStatus[keyof typeof ContractStatus];

export const MilestoneStatus = {
  Draft: 0,
  AwaitingFunding: 1,
  FundingProcessing: 2,
  FundedInProgress: 3,
  Submitted: 4,
  AcceptedHold: 5,
  Disputed: 6,
  Released: 7,
  Refunded: 8,
  Cancelled: 9,
} as const;
export type MilestoneStatus = typeof MilestoneStatus[keyof typeof MilestoneStatus];

export const MilestoneFundingStatus = {
  Unfunded: 0,
  Processing: 1,
  Funded: 2,
  Settled: 3,
} as const;
export type MilestoneFundingStatus = typeof MilestoneFundingStatus[keyof typeof MilestoneFundingStatus];

export const EscrowHoldStatus = {
  Funded: 0,
  Frozen: 1,
  Released: 2,
  Refunded: 3,
} as const;
export type EscrowHoldStatus = typeof EscrowHoldStatus[keyof typeof EscrowHoldStatus];

export interface ContractMilestoneDto {
  id: string;
  orderNumber: number;
  title: string;
  description: string | null;
  amount: number;
  durationDays: number | null;
  dueDate: string | null;
  status: MilestoneStatus;
  fundingStatus: MilestoneFundingStatus;
  escrowHoldId: string | null;
  fundedAt: string | null;
  submittedAt: string | null;
  autoAcceptEligibleAt: string | null;
  holdExpiresAt: string | null;
  netLawyerAmount: number | null;
  version: string;
}

export interface ContractPaymentDto {
  id: string;
  milestoneId: string;
  grossAmount: number;
  platformFee: number;
  netAmount: number;
  currency: string;
  status: EscrowHoldStatus;
  holdExpiresAt: string | null;
  settledAt: string | null;
}

export interface ContractDetailDto {
  id: string;
  proposalId: string;
  legalCaseId: string;
  clientUserId: string;
  lawyerUserId: string;
  title: string;
  termsAndConditions: string;
  currency: string;
  status: ContractStatus;
  acceptedByClientAt: string | null;
  acceptedByLawyerAt: string | null;
  activatedAt: string | null;
  completedAt: string | null;
  terminatedAt: string | null;
  currentMilestoneTotal: number;
  version: string;
  milestones: ContractMilestoneDto[];
  payments: ContractPaymentDto[];
  permittedActions: string[];
}

export interface ContractSummaryDto {
  id: string;
  legalCaseId: string;
  clientUserId: string;
  lawyerUserId: string;
  title: string;
  currency: string;
  status: ContractStatus;
  activatedAt: string | null;
  completedAt: string | null;
}

export interface ContractStateHistoryDto {
  id: string;
  previousStatus: ContractStatus | null;
  newStatus: ContractStatus;
  trigger: string;
  actorUserId: string | null;
  reason: string | null;
  createdAt: string;
}

export interface CreateContractRequest {
  proposalId: string;
  title: string;
  termsAndConditions: string;
}

export interface UpdateContractRequest {
  title: string;
  termsAndConditions: string;
}

export interface TerminateContractRequest {
  reason: string;
}

export interface ContractActionResultDto {
  entityId: string;
  status: string; // E.g., 'Draft', 'Active'
  occurredAt: string;
}

// Common generic wrappers 
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: string[] | null;
  statusCode: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
}
