export type CaseStatus = 
  | 'Submitted'      // 1
  | 'Reviewed'       // 2
  | 'FinalSubmitted' // 3
  | 'Analyzed'       // 4
  | 'Matched'        // 5
  | 'Assigned'       // 6
  | 'Closed';        // 7

export interface CaseListItemDto {
  id: string;
  title: string;
  status: CaseStatus;
  createdAt: string;
  documentCount: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string | null;
  errors: string[] | null;
  statusCode: number;
}

export type ReviewPointType = 'Strength' | 'Weakness' | 'Suggestion' | 'MissingCaseInfo' | 'MissingCaseDoc';

export interface ReviewPointDto {
  id: string;
  description: string;
  type: ReviewPointType;
}

export interface CaseReviewReportDto {
  id: string;
  caseId: string;
  isLatest: boolean;
  createdAt: string;
  reviewPoints: ReviewPointDto[];
}
