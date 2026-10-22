export interface Student {
  studentId: number;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  mobile: string;
  countryId: number;
  countryName: string;
  stateId: number;
  stateName: string;
  cityId: number;
  cityName: string;
  address: string;
  createdAt: string;
  updatedAt: string;
}

export interface StudentRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  mobile: string;
  countryId: number;
  stateId: number;
  cityId: number;
  address: string;
}

export interface StudentQuery {
  search?: string;
  countryId?: number;
  stateId?: number;
  cityId?: number;
  page: number;
  pageSize: number;
}