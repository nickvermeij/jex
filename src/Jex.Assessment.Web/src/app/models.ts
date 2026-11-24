export interface Address {
  street: string;
  houseNumber: string;
  postalCode: string;
  city: string;
  countryCode: string;
  latitude: number | null;
  longitude: number | null;
}

export interface PublicVacancy {
  id: string;
  title: string;
  description: string;
}

export interface PublicCompany {
  id: string;
  name: string;
  address: Address;
  vacancies: PublicVacancy[];
  distanceKm?: number;
}

export interface CreatedVacancy {
  id: string;
  companyId: string;
  title: string;
  description: string | null;
  status: 'Draft' | 'Active' | 'Closed';
  createdAtUtc: string;
}
