# ISMEDU Student Management Portal

A web application to manage **Countries → States → Cities** and **Students**, with JWT login.
The frontend is **Angular**, the backend is an **ASP.NET Core Web API**, and the data is stored in **MySQL**.

---

## Features

- **Login with JWT** — passwords stored as BCrypt hashes
- **Dashboard** — totals for countries, states, cities and students, with quick actions
- **Countries, States, Cities** — list with search, filters and pagination, able to add, edit and delete
- **Students** — list with search and Country/State/City filters. Registration form with
  linked Country → State → City dropdowns and full validation
- **Validation on both sides** — Angular checks forms before sending, the API checks everything again
- **Safe deletes** — a country, state or city that is still used cannot be deleted
  (a clear message is shown)
- **Consistent errors** — every API error uses the same JSON shape, and the UI shows each message
  under the matching field
- **Responsive** — works on phone, tablet and desktop


---


## Requirements

Install these first:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22 or newer](https://nodejs.org/) (includes npm)
- [MySQL 8.4](https://dev.mysql.com/downloads/mysql/) (any MySQL 8+ works)
- A MySQL client to run the scripts, e.g. MySQL Workbench

---

## Setup

### 1. Database

Run the two scripts **in this order**:

1. `database/schema.sql` — creates the `ismedu_db` database and its tables
2. `database/seed.sql` — adds sample countries, states, cities, students and the login user


### 2. Backend (API)

Passwords and keys are NOT stored in the code. Set them once with .NET User Secrets.

Open a terminal in the `backend` folder:

```bash
cd backend
```

**Connection string** — replace `YOUR_MYSQL_PASSWORD` with your MySQL root password:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=ismedu_db;User ID=root;Password=YOUR_MYSQL_PASSWORD;"
```

**JWT signing key** — must be long and random (at least 32 characters). To generate one in PowerShell:

```powershell
$b = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

Then save it:

```bash
dotnet user-secrets set "Jwt:Key" "PASTE_THE_GENERATED_KEY_HERE"
```

**Run the API:**

```bash
dotnet run
```

The terminal shows the address, e.g. `http://localhost:5161`.

- Swagger UI: `http://localhost:5161/swagger`

### 3. Frontend (Angular)

Open a **second** terminal in the `frontend` folder:

```bash
cd frontend
npm install
npm start
```

Open **http://localhost:4200** in your browser.

If your API runs on a different port than `5161`, change `apiUrl` in
`frontend/src/environments/environment.development.ts`.

---

## Development login

Email = `admin@ismedu.com` 
Password = `Admin@123`

> This account is created by `seed.sql` for **development and testing only**. 

---

## Using Swagger with a token

Most endpoints require a token.

1. Open `/swagger` and run `POST /api/auth/login` with the development login above
2. Copy the `token` value from the response
3. Click **Authorize**, paste the token, click **Authorize**
4. All requests now include the token


---


## Test evidence

Screenshots are in [`docs/screenshots/`](docs/screenshots/).

01. Login page 
02. Wrong password → "Invalid email or password." 
03. Dashboard with totals 
04. Countries list 
05. Duplicate country → error under the name field
06. Deleting a country that is in use → blocked with a clear message
07. States list with country filter 
08. City form with linked Country → State dropdowns
09. Students list 
10. Student form validation errors 
11. Duplicate student email/mobile → errors under the fields 
12. Phone view 