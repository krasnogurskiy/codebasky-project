# Codebasky Project

`Codebasky` is now a working course-project monorepo with:

- `backend/` ASP.NET Core modular monolith
- `frontend/` React + TypeScript + Vite SPA
- `.github/workflows/` CI pipeline
- `docs/` delivery artifacts for phases `1` to `6`

## Application Structure

- `backend/src/Web` HTTP API, auth, Swagger and SignalR hub
- `backend/src/Application` services and DTO contracts
- `backend/src/Domain` core entities and rules
- `backend/src/Infrastructure` EF Core persistence and seed data
- `frontend/src` routes, session handling, realtime updates and UI pages

## Local Run

Backend:

```powershell
dotnet run --project backend/src/Web/Codebasky.Web.csproj
```

Frontend:

```powershell
cd frontend
npm install
npm run dev
```

Default local URLs:

- frontend: `http://localhost:5173`
- backend: `http://localhost:5255`
- Swagger UI: `http://localhost:5255/api`

## Automated Verification

Backend:

```powershell
dotnet test backend/Codebasky.sln
```

Frontend:

```powershell
cd frontend
npm run lint
npm run build
npm run test
npm run test:e2e
```

Implemented automated coverage:

- backend domain unit tests: `24`
- backend application unit tests: `5`
- backend API functional tests: `19`
- frontend component/integration tests: `12`
- Playwright e2e tests: `5`

Total automated tests: `65`

## Delivery Artifacts

### Phase 1. Команда та ролі

- [Team Charter](docs/01-team/team-charter.pdf)

### Phase 2. Цілі проєкту та продуктове бачення

- [Project Charter and Product Vision](docs/02-vision/project-charter.pdf)

### Phase 3. Планування та управління ресурсами

- [Project Plan](docs/03-planning/project-plan.pdf)
- [Gantt Chart source](docs/03-planning/Gantt%20chart.txt)

### Phase 4. Requirements Engineering

- [SRS](docs/SRS.pdf)
- [UML Use Case source](docs/UML%20diadram.txt)
- [Sequence Diagram source](docs/Sequence%20diagram.txt)
- [Concurrency Diagrams](docs/Concurrency%20Diagrams/)
- [User Story Backlog](docs/04-requirements/user-story-backlog.pdf)
- [Traceability Matrix](docs/04-requirements/traceability-matrix.csv)

### Phase 5. UI/UX Design and Prototype

- [Design Description](docs/05-ui-ux/design-description.pdf)
- [User Flows](docs/05-ui-ux/user-flows.pdf)
- [Wireframes](docs/05-ui-ux/wireframes.pdf)

### Phase 6. Testing Strategy

- [Test Strategy](docs/06-testing/test-strategy.pdf)
- [Smoke Test Plan](docs/06-testing/smoke-test-plan.pdf)
- [Manual Test Cases](docs/06-testing/manual-test-cases.csv)
- [Bug List](docs/06-testing/bug-list.csv)
- [Test Summary Report](docs/06-testing/test-summary-report.pdf)
