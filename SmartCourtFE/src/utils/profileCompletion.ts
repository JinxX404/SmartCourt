import type { User } from '../features/auth/store/useAuthStore';
import type { LawyerProfile, ClientProfile } from '../features/users/api/usersApi';

export const calculateProfileCompletion = (
  user: User | null,
  profile: LawyerProfile | ClientProfile | null,
  documents: any[]
): number => {
  if (!user) return 0;
  
  // If Admin or any other role, they get full 100% statically
  if (user.role !== 'Lawyer' && user.role !== 'Client') {
    return 100;
  }

  let totalPoints = 0;

  if (user.role === 'Lawyer') {
    const p = profile as LawyerProfile;
    // Contact Info (20%) - Awarded instantly
    if (p?.phoneNumber) totalPoints += 10;
    if (p?.email) totalPoints += 10;

    // Only award profile info points if the profile has been approved by admin (Active)
    if (user.status === 'Active') {
      // Personal Info (20%)
      if (p?.nationalNumber) totalPoints += 5;
      if (p?.dateOfBirth) totalPoints += 5;
      if (p?.governorate) totalPoints += 5;
      if (p?.city) totalPoints += 5;

      // Professional Info (30%)
      if (p?.level) totalPoints += 10;
      if (p?.bio) totalPoints += 10;
      if (p?.specializations && p.specializations.length > 0) {
        // 5 points per specialization, max 10 points (2 specializations)
        totalPoints += Math.min(p.specializations.length * 5, 10);
      }
    }

    // Documents (30%)
    const hasVerifiedPicture = documents?.some((d: any) => 
      (d.documentType === 'OfficialProfilePicture' || d.documentType === 7) && (d.status === 'Verified' || d.status === 2) && d.isCurrent
    );
    if (hasVerifiedPicture) totalPoints += 15;

    const hasSyndicateCard = documents?.some((d: any) => 
      (d.documentType === 'BarAssociationCardFront' || d.documentType === 3) && (d.status === 'Verified' || d.status === 2) && d.isCurrent
    );
    if (hasSyndicateCard) totalPoints += 15;

  } else if (user.role === 'Client') {
    const p = profile as ClientProfile;
    // Contact Info (30%) - Awarded instantly
    if (p?.phoneNumber) totalPoints += 15;
    if (p?.email) totalPoints += 15;

    // Only award profile info points if the profile has been approved by admin (Active)
    if (user.status === 'Active') {
      // Personal Info (40%)
      if (p?.nationalNumber) totalPoints += 10;
      if (p?.dateOfBirth) totalPoints += 10;
      if (p?.governorate) totalPoints += 10;
      if (p?.city) totalPoints += 10;
    }

    // Documents (30%)
    const hasVerifiedPicture = documents?.some((d: any) => 
      (d.documentType === 'SelfieWithId' || d.documentType === 5) && (d.status === 'Verified' || d.status === 2) && d.isCurrent
    );
    if (hasVerifiedPicture) totalPoints += 15;

    const hasNationalId = documents?.some((d: any) => 
      (d.documentType === 'NationalIdFront' || d.documentType === 1) && (d.status === 'Verified' || d.status === 2) && d.isCurrent
    );
    if (hasNationalId) totalPoints += 15;
  }

  return Math.min(totalPoints, 100);
};
