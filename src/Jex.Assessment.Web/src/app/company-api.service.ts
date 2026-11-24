import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreatedVacancy, PublicCompany } from './models';

@Injectable({ providedIn: 'root' })
export class CompanyApiService {
  private readonly http = inject(HttpClient);
  private readonly companiesUrl = '/api/v1/companies';
  private readonly vacanciesUrl = '/api/v1/vacancies';

  getCompanies(): Observable<PublicCompany[]> {
    return this.http.get<PublicCompany[]>(this.companiesUrl);
  }

  getNearbyCompanies(latitude: number, longitude: number, radiusKm: number): Observable<PublicCompany[]> {
    const params = new HttpParams()
      .set('latitude', latitude)
      .set('longitude', longitude)
      .set('radiusKm', radiusKm);

    return this.http.get<PublicCompany[]>(`${this.companiesUrl}/nearby`, { params });
  }

  createVacancy(companyId: string, title: string): Observable<CreatedVacancy> {
    return this.http.post<CreatedVacancy>(`${this.vacanciesUrl}/${companyId}/vacancies`, { title });
  }
}
