import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CompanyApiService } from './company-api.service';

describe('CompanyApiService', () => {
  let service: CompanyApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CompanyApiService, provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(CompanyApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads companies from the current API route', () => {
    service.getCompanies().subscribe();

    const request = http.expectOne('/api/v1/companies');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('loads nearby companies with the search parameters', () => {
    service.getNearbyCompanies(51.9244, 4.4777, 25).subscribe();

    const request = http.expectOne(
      (candidate) =>
        candidate.url === '/api/v1/companies/nearby' &&
        candidate.params.get('latitude') === '51.9244' &&
        candidate.params.get('longitude') === '4.4777' &&
        candidate.params.get('radiusKm') === '25',
    );
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('creates a vacancy through the current API route', () => {
    const companyId = '019ab8f5-2454-781f-8db7-190f5183a0c1';
    service.createVacancy(companyId, 'Senior .NET Developer').subscribe();

    const request = http.expectOne(`/api/v1/vacancies/${companyId}/vacancies`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ title: 'Senior .NET Developer' });
    request.flush({
      id: '019ab8f5-2454-781f-8db7-190f5183a0c2',
      companyId,
      title: 'Senior .NET Developer',
      description: null,
      status: 'Draft',
      createdAtUtc: '2026-11-24T12:00:00Z',
    });
  });
});
