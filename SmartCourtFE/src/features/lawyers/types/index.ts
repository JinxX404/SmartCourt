export interface LawyerSpecializationDto {
  specialization: number;
  yearsOfExperience: number;
  casesHandled: number;
}

export interface LawyerSearchDto {
  id: string;
  name: string;
  gender: string | null;
  level: number; // 1: General, 2: Primary, 3: Appeal, 4: Cassation, 5: Consultant
  bio: string | null;
  isAvailable: boolean;
  profilePictureUrl: string | null;
  specialization: number | null;
  specializations: LawyerSpecializationDto[];
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

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string | null;
  errors: string[] | null;
  statusCode: number;
}

export interface LawyerPublicProfileDto {
  id: string;
  name: string;
  gender: number | null;
  level: number;
  bio: string | null;
  governorate: string | null;
  city: string | null;
  isAvailable: boolean;
  profilePictureUrl: string | null;
  yearsOfExperience: number;
  specializationName: string | null;
  specializations: LawyerSpecializationDto[];
  rating: number;
}
