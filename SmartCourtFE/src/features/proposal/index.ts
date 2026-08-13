// API
export { ProposalApi } from './api/proposalApi';

// Types
export type {
  ProposalStatus,
  ProposalAction,
  ProposalDetailDto,
  ProposalListItem,
  ProposalPageDto,
  ProposalSlotAvailabilityDto,
  CreateProposalRequest,
  RejectProposalRequest,
  CancelProposalRequest,
  TerminateProposalRequest,
  ListProposalsParams,
  ApiResponse,
} from './types/proposal.types';

// Components
export { SendProposalModal } from './components/SendProposalModal';
export { ProposalCard } from './components/ProposalCard';
export { LawyerProposalsList } from './components/LawyerProposalsList';
export { CaseProposalsList } from './components/CaseProposalsList';
export { ProposalDetail } from './components/ProposalDetail';
