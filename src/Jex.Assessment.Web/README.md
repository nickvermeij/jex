# JEX Assessment Web

A responsive Angular and Tailwind CSS client for the versioned JEX assessment API.

## Development

Start the API from the repository root:

```bash
dotnet run --project src/Jex.Assessment.Api
```

Then start the frontend:

```bash
cd src/Jex.Assessment.Web
npm install
npm start
```

Open `http://localhost:4200`.

The Angular development server uses `proxy.conf.json` to proxy `/api` to `http://localhost:5023`. Keep that API port aligned with `Jex.Assessment.Api/Properties/launchSettings.json`.

## Commands

```bash
npm start       # Development server with API proxy
npm run build   # Optimized production build
npm test        # Unit tests
```

## Features

- Lists companies with active vacancies
- Searches within a radius of Rotterdam, Amsterdam or the user's browser location
- Shows distance returned by the API
- Creates a public vacancy as a draft
- Provides responsive loading, error and empty states

The API contract and domain types are represented in `src/app/models.ts`; HTTP access is isolated in `src/app/company-api.service.ts`.
