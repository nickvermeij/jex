import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { CompanyApiService } from './company-api.service';
import { PublicCompany } from './models';

@Component({
  imports: [FormsModule],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly api = inject(CompanyApiService);

  protected readonly companies = signal<PublicCompany[]>([]);
  protected readonly loading = signal(true);
  protected readonly locating = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly locationLabel = signal('Heel Nederland');
  protected readonly radiusKm = signal(25);
  protected readonly vacancyCount = computed(() =>
    this.companies().reduce((total, company) => total + company.vacancies.length, 0),
  );

  protected readonly selectedCompany = signal<PublicCompany | null>(null);
  protected readonly vacancyTitle = signal('');
  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly notification = signal<string | null>(null);

  ngOnInit(): void {
    this.showAll();
  }

  protected showAll(): void {
    this.locationLabel.set('Heel Nederland');
    this.load(this.api.getCompanies());
  }

  protected searchPreset(latitude: number, longitude: number, label: string): void {
    this.locationLabel.set(`${label} · ${this.radiusKm()} km`);
    this.load(this.api.getNearbyCompanies(latitude, longitude, this.radiusKm()));
  }

  protected useCurrentLocation(): void {
    if (!navigator.geolocation) {
      this.error.set('Je browser ondersteunt geen locatiebepaling.');
      return;
    }

    this.locating.set(true);
    this.error.set(null);
    navigator.geolocation.getCurrentPosition(
      ({ coords }) => {
        this.locating.set(false);
        this.searchPreset(coords.latitude, coords.longitude, 'In mijn buurt');
      },
      () => {
        this.locating.set(false);
        this.error.set('We konden je locatie niet ophalen. Controleer de toestemming van je browser.');
      },
      { enableHighAccuracy: false, timeout: 10_000 },
    );
  }

  protected openVacancyForm(company: PublicCompany): void {
    this.selectedCompany.set(company);
    this.vacancyTitle.set('');
    this.formError.set(null);
  }

  protected closeVacancyForm(): void {
    if (!this.submitting()) {
      this.selectedCompany.set(null);
    }
  }

  protected submitVacancy(): void {
    const company = this.selectedCompany();
    const title = this.vacancyTitle().trim();
    if (!company || this.submitting()) return;

    if (!title) {
      this.formError.set('Vul een functietitel in.');
      return;
    }

    this.submitting.set(true);
    this.formError.set(null);
    this.api
      .createVacancy(company.id, title)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          this.selectedCompany.set(null);
          this.notification.set(`Je vacature voor ${company.name} is als concept opgeslagen.`);
          window.setTimeout(() => this.notification.set(null), 5_000);
        },
        error: () => this.formError.set('Opslaan is niet gelukt. Probeer het opnieuw.'),
      });
  }

  protected formatAddress(company: PublicCompany): string {
    const address = company.address;
    return `${address.street} ${address.houseNumber}, ${address.postalCode} ${address.city}`;
  }

  private load(request: ReturnType<CompanyApiService['getCompanies']>): void {
    this.loading.set(true);
    this.error.set(null);
    request.pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (companies) => this.companies.set(companies),
      error: () => {
        this.companies.set([]);
        this.error.set('De vacatures konden niet worden opgehaald. Is de API gestart?');
      },
    });
  }
}
