export type ProposalStatus =
  | "Pending"
  | "Accepted"
  | "Rejected"
  | "Cancelled"
  | "Expired"
  | "Terminated"
  | "Superseded";

export type ProposalAction =
  | "Accept"
  | "Reject"
  | "Cancel"
  | "TerminateProposal"
  | "CreateContract"
  | "ViewContract"
  | "OpenChat"
  | "ViewChatHistory";

export interface ProposalListItem {
  id: string;
  legalCaseId: string;
  caseTitle: string;
  clientUserId: string;
  clientName: string;
  lawyerUserId: string;
  lawyerName: string;
  status: ProposalStatus;
  caseStatus: string;
  assignedLawyerUserId: string | null;
  isAssignedLawyer: boolean;
  contractId: string | null;
  contractStatus: string | null;
  conversationId: string | null;
  conversationStatus: "Open" | "Closed" | null;
  canChat: boolean;
  permittedActions: ProposalAction[];
  createdAt: string;
  respondedAt: string | null;
  expiresAt: string | null;
  closedAt: string | null;
  closedByUserId: string | null;
}

export interface ProposalDetailDto extends ProposalListItem {
  message: string;
  decisionReason: string | null;
  updatedAt: string;
}

export interface ProposalSlotAvailabilityDto {
  legalCaseId: string;
  activeProposalCount: number;
  proposalLimit: number;
  availableProposalSlots: number;
  canSendProposal: boolean;
}

export interface ProposalPageDto {
  items: ProposalListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
}

export interface CreateProposalRequest {
  legalCaseId: string;
  lawyerUserId: string;
  message: string;
}

export interface RejectProposalRequest {
  reason: string;
}

export interface CancelProposalRequest {
  reason: string;
}

export interface TerminateProposalRequest {
  reason: string;
}

export interface ListProposalsParams {
  statuses?: ProposalStatus[];
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string | null;
  errors: string[] | null;
  data: T;
}
