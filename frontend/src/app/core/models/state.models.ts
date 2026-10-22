export interface State {
  stateId: number;
  stateName: string;
  countryId: number;
  countryName: string;
  isActive: boolean;
}

export interface StateRequest {
  stateName: string;
  countryId: number;
  isActive: boolean;
}

export interface StateOption {
  stateId: number;
  stateName: string;
}

export interface StateQuery {
  search?: string;
  countryId?: number;
  isActive?: boolean;
  page: number;
  pageSize: number;
}