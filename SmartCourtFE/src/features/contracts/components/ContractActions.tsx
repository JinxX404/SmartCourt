import { useState } from 'react';
import { 
  useAcceptContract, 
  useTerminateContract, 
  useUpdateContractDraft 
} from '../hooks/useContractQueries';
import { type ContractDetailDto, ContractStatus } from '../types/contract.types';
import toast from 'react-hot-toast';

interface ContractActionsProps {
  contract: ContractDetailDto;
  isLawyer: boolean;
}

export function ContractActions({ contract, isLawyer }: ContractActionsProps) {
  const { mutate: accept, isPending: isAccepting } = useAcceptContract(contract.id);
  const { mutate: terminate, isPending: isTerminating } = useTerminateContract(contract.id);
  const { mutate: updateDraft, isPending: isUpdating } = useUpdateContractDraft(contract.id);
  
  const [isEditing, setIsEditing] = useState(false);
  const [title, setTitle] = useState(contract.title);
  const [terms, setTerms] = useState(contract.termsAndConditions);

  // The backend explicitly warns that permittedActions might incorrectly tell a Client they can "Update".
  // We hardcode the rule that only Lawyers can update Drafts.
  const canUpdate = contract.status === ContractStatus.Draft && isLawyer;
  const canAccept = contract.status === ContractStatus.Draft && 
    (isLawyer ? !contract.acceptedByLawyerAt : !contract.acceptedByClientAt);
  // Termination depends on status, but broadly available unless completed/terminated.
  const canTerminate = contract.status !== ContractStatus.Completed && contract.status !== ContractStatus.Terminated;

  const handleUpdate = () => {
    updateDraft(
      { version: contract.version, data: { title, termsAndConditions: terms } },
      { onSuccess: () => setIsEditing(false) }
    );
  };

  const handleAccept = () => {
    accept({ version: contract.version });
  };

  const handleTerminate = () => {
    // Ideally this opens a modal to ask for the reason
    const reason = prompt('Please provide a reason for termination:');
    if (!reason) return;

    if (reason.length > 2000) {
      toast.error('Reason must not exceed 2000 characters.');
      return;
    }
    
    terminate({ version: contract.version, data: { reason } });
  };

  if (isEditing) {
    return (
      <div className="flex flex-col gap-4 mt-6 p-6 border rounded-lg shadow-sm bg-white">
        <h3 className="font-semibold text-lg">Edit Contract Draft</h3>
        <input 
          value={title} 
          onChange={(e) => setTitle(e.target.value)} 
          className="border p-2 rounded" 
          placeholder="Title"
        />
        <textarea 
          value={terms} 
          onChange={(e) => setTerms(e.target.value)} 
          className="border p-2 rounded h-32" 
          placeholder="Terms and Conditions"
        />
        <div className="flex gap-2 justify-end">
          <button 
            className="px-4 py-2 border rounded hover:bg-gray-50"
            onClick={() => {
              setIsEditing(false);
              setTitle(contract.title);
              setTerms(contract.termsAndConditions);
            }}
            disabled={isUpdating}
          >
            Cancel
          </button>
          <button 
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
            onClick={handleUpdate}
            disabled={isUpdating}
          >
            {isUpdating ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex gap-3 mt-6">
      {canUpdate && (
        <button 
          onClick={() => setIsEditing(true)}
          className="px-4 py-2 border border-blue-600 text-blue-600 rounded hover:bg-blue-50 transition-colors"
        >
          Edit Draft
        </button>
      )}

      {canAccept && (
        <button 
          onClick={handleAccept}
          disabled={isAccepting}
          className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50 transition-colors"
        >
          {isAccepting ? 'Accepting...' : 'Accept Contract'}
        </button>
      )}

      {canTerminate && (
        <button 
          onClick={handleTerminate}
          disabled={isTerminating}
          className="px-4 py-2 border border-red-600 text-red-600 rounded hover:bg-red-50 disabled:opacity-50 transition-colors"
        >
          {isTerminating ? 'Terminating...' : 'Terminate'}
        </button>
      )}
    </div>
  );
}
