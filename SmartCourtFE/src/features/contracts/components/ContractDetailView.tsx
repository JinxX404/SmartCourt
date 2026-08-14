import { useContract, useContractNotificationsSync } from '../hooks/useContractQueries';
import { ContractActions } from './ContractActions';
import { ContractStatus } from '../types/contract.types';
import { useAuthStore } from '../../auth/store/useAuthStore';

// Utility to render status nicely
const renderStatus = (status: ContractStatus) => {
  const statusMap = {
    [ContractStatus.Draft]: { label: 'Draft', color: 'bg-yellow-100 text-yellow-800' },
    [ContractStatus.Active]: { label: 'Active', color: 'bg-blue-100 text-blue-800' },
    [ContractStatus.SuspendedByDispute]: { label: 'Suspended (Disputed)', color: 'bg-red-100 text-red-800' },
    [ContractStatus.Completed]: { label: 'Completed', color: 'bg-green-100 text-green-800' },
    [ContractStatus.Terminated]: { label: 'Terminated', color: 'bg-gray-100 text-gray-800' },
  };

  const info = statusMap[status] || { label: 'Unknown', color: 'bg-gray-100' };
  
  return (
    <span className={`px-2 py-1 rounded text-xs font-semibold ${info.color}`}>
      {info.label}
    </span>
  );
};

export function ContractDetailView({ contractId }: { contractId: string }) {
  const { user } = useAuthStore();
  
  const { data: response, isLoading, isError, error } = useContract(contractId);
  const contract = response?.data;
  
  const isLawyer = contract?.lawyerUserId === user?.id;

  // Initialize the SignalR notifications sync for this specific contract
  useContractNotificationsSync(contractId);

  if (isLoading) return <div className="p-8 text-center animate-pulse">Loading contract details...</div>;
  if (isError) return <div className="p-8 text-center text-red-500">Failed to load contract: {(error as any)?.message}</div>;

  if (!contract) return null;

  return (
    <div className="max-w-4xl mx-auto p-6 space-y-8">
      <div className="flex justify-between items-start border-b pb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{contract.title}</h1>
          <p className="text-sm text-gray-500 mt-1">ID: {contract.id}</p>
        </div>
        <div>
          {renderStatus(contract.status)}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-8">
        <section className="space-y-4">
          <h2 className="text-xl font-semibold text-gray-800">Parties</h2>
          <div className="bg-gray-50 p-4 rounded-lg space-y-2">
            <p><span className="font-medium">Client ID:</span> {contract.clientUserId}</p>
            <p><span className="font-medium">Lawyer ID:</span> {contract.lawyerUserId}</p>
          </div>
        </section>
        
        <section className="space-y-4">
          <h2 className="text-xl font-semibold text-gray-800">Acceptance Status</h2>
          <div className="bg-gray-50 p-4 rounded-lg space-y-2">
            <p>
              <span className="font-medium">Client: </span> 
              {contract.acceptedByClientAt ? (
                <span className="text-green-600">Accepted on {new Date(contract.acceptedByClientAt).toLocaleString()}</span>
              ) : (
                <span className="text-gray-500">Pending</span>
              )}
            </p>
            <p>
              <span className="font-medium">Lawyer: </span> 
              {contract.acceptedByLawyerAt ? (
                <span className="text-green-600">Accepted on {new Date(contract.acceptedByLawyerAt).toLocaleString()}</span>
              ) : (
                <span className="text-gray-500">Pending</span>
              )}
            </p>
          </div>
        </section>
      </div>

      <section className="space-y-4">
        <h2 className="text-xl font-semibold text-gray-800">Terms and Conditions</h2>
        <div className="bg-white border rounded-lg p-6 min-h-50 whitespace-pre-wrap text-gray-700 leading-relaxed shadow-sm">
          {/* Note: The backend returns untrusted HTML/text. React escapes it by default when using standard children interpolation, which is safe. */}
          {contract.termsAndConditions}
        </div>
      </section>

      {/* Contract Actions Component (handles edit, accept, terminate) */}
      <ContractActions contract={contract} isLawyer={isLawyer} />
    </div>
  );
}
