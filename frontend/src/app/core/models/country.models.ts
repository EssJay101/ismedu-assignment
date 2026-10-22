export interface Country {
  countryId: number;
  countryName: string;
  countryCode: string | null;
  isActive: boolean;
}

export interface CountryRequest {
  countryName: string;
  countryCode: string | null;
  isActive: boolean;
}

export interface CountryOption {
  countryId: number;
  countryName: string;
}

export interface CountryQuery {
  search?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
}