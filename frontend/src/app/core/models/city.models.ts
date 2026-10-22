export interface City {
  cityId: number;
  cityName: string;
  stateId: number;
  stateName: string;
  countryId: number;
  countryName: string;
  isActive: boolean;
}

export interface CityRequest {
  cityName: string;
  stateId: number;
  isActive: boolean;
}

export interface CityOption {
  cityId: number;
  cityName: string;
}

export interface CityQuery {
  search?: string;
  countryId?: number;
  stateId?: number;
  isActive?: boolean;
  page: number;
  pageSize: number;
}