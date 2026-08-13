import { useParams } from 'react-router-dom';
import { ContractDetailView } from '../features/contracts/components/ContractDetailView';

export function ContractDetailPage() {
  const { id } = useParams<{ id: string }>();

  if (!id) {
    return <div className="p-8 text-center text-red-500">No Contract ID provided</div>;
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <ContractDetailView contractId={id} />
    </div>
  );
}
