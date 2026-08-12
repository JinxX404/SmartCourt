export interface LawyerSearchDto {
  id: string;
  name: string;
  gender: string | null;
  level: number; // 1: General, 2: Primary, 3: Appeal, 4: Cassation, 5: Consultant
  bio: string | null;
  isAvailable: boolean;
  profilePictureUrl: string | null;
  specialization: number | null;
  rating: number;
  governorate: string | null;
}

export interface SearchLawyersRequest {
  searchTerm?: string;
  governorate?: string;
  level?: number;
  specialization?: number;
  minRating?: number;
  isAvailable?: boolean;
  sortBy?: number;
  sortDirection?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface PagedResponse<T> {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  success: boolean;
  data: T[];
  message: string | null;
  errors: string[] | null;
  statusCode: number;
}
