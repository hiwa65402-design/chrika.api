# Chrika.Api - Social Media API

پرۆژەیەکی ASP.NET Core Web API بۆ سۆشیال میدیا کە تایبەتمەندییە بنەڕەتییەکانی بەڕێوەبردنی بەکارهێنەران لەخۆدەگرێت.

## تایبەتمەندییەکان

- **بەڕێوەبردنی بەکارهێنەران**: دروستکردن، خوێندنەوە، نوێکردنەوە و سڕینەوەی بەکارهێنەران
- **سیستەمی چوونەژوورەوە**: Authentication بە بەکارهێنانی username یان email
- **API Documentation**: Swagger/OpenAPI integration
- **CORS Support**: پشتگیری لە Cross-Origin Resource Sharing
- **Docker Support**: ئامادە بۆ containerization
- **Railway Deployment**: ڕێکخراو بۆ بڵاوکردنەوە لەسەر Railway.com

## ستراکچەری پرۆژە

```
chrika.api/
├── src/
│   └── Chrika.Api/
│       ├── Controllers/          # API Controllers
│       ├── Models/              # Data Models
│       ├── DTOs/                # Data Transfer Objects
│       ├── Services/            # Business Logic Services
│       └── Program.cs           # Application Entry Point
├── Dockerfile                   # Docker Configuration
├── .dockerignore               # Docker Ignore Rules
├── Chrika.Api.sln             # Solution File
└── README.md                   # Project Documentation
```

## API Endpoints

### بەکارهێنەران (Users)

- `GET /api/users` - وەرگرتنی هەموو بەکارهێنەران
- `GET /api/users/{id}` - وەرگرتنی بەکارهێنەر بە ID
- `GET /api/users/{id}/profile` - وەرگرتنی پرۆفایلی بەکارهێنەر
- `GET /api/users/username/{username}` - وەرگرتنی بەکارهێنەر بە username
- `POST /api/users` - دروستکردنی بەکارهێنەری نوێ
- `PUT /api/users/{id}` - نوێکردنەوەی بەکارهێنەر
- `DELETE /api/users/{id}` - سڕینەوەی بەکارهێنەر
- `POST /api/users/authenticate` - چوونەژوورەوە
- `GET /api/users/check-username/{username}` - پشکنینی بەردەستبوونی username
- `GET /api/users/check-email/{email}` - پشکنینی بەردەستبوونی email

## چۆنیەتی بەکارهێنان

### پێداویستییەکان

- .NET 8.0 SDK
- Docker (ئەگەر بتەوێت containerize بکەیت)

### کارپێکردن لە Local

1. **Clone کردنی پرۆژە:**
   ```bash
   git clone <repository-url>
   cd chrika.api
   ```

2. **Restore کردنی dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build کردنی پرۆژە:**
   ```bash
   dotnet build
   ```

4. **Run کردنی پرۆژە:**
   ```bash
   dotnet run --project src/Chrika.Api
   ```

5. **دەستگەیشتن بە API:**
   - API Base URL: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

### بەکارهێنان لەگەڵ Docker

1. **Build کردنی Docker Image:**
   ```bash
   docker build -t chrika-api .
   ```

2. **Run کردنی Container:**
   ```bash
   docker run -p 5000:5000 chrika-api
   ```

## بڵاوکردنەوە لەسەر Railway

### هەنگاوەکان:

1. **دروستکردنی GitHub Repository:**
   - پرۆژەکە upload بکە بۆ GitHub

2. **پەیوەستکردن بە Railway:**
   - بچۆ بۆ [railway.com](https://railway.com)
   - حسابێک دروست بکە یان بچۆژوورەوە
   - "New Project" کلیک بکە
   - "Deploy from GitHub repo" هەڵبژێرە

3. **ڕێکخستنی پرۆژە:**
   - Repository ی خۆت هەڵبژێرە
   - Railway خۆکارانە Dockerfile دەناسێتەوە
   - Environment variables دابنێ ئەگەر پێویست بوو

4. **Deploy کردن:**
   - Railway خۆکارانە پرۆژەکەت build و deploy دەکات
   - URL ی public وەردەگریت بۆ API ەکەت

### Environment Variables (ئەگەر پێویست بوو)

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:5000
```

## نموونەی بەکارهێنان

### دروستکردنی بەکارهێنەری نوێ

```bash
curl -X POST "http://localhost:5000/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "password": "SecurePassword123",
    "dateOfBirth": "1990-01-01T00:00:00Z"
  }'
```

### چوونەژوورەوە

```bash
curl -X POST "http://localhost:5000/api/users/authenticate" \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "john_doe",
    "password": "SecurePassword123"
  }'
```

## تایبەتمەندییە داهاتووەکان

- پەیوەستکردن بە داتابەیسی ڕاستەقینە (PostgreSQL/SQL Server)
- JWT Authentication
- File Upload بۆ Profile Pictures
- Post و Comment Management
- Follow/Unfollow System
- Real-time Notifications

## یارمەتی و پشتگیری

ئەگەر هیچ پرسیارێکت هەیە یان کێشەیەکت هەیە، تکایە issue ێک دروست بکە لە GitHub repository ەکەدا.

---

**تێبینی:** ئەم پرۆژەیە بۆ مەبەستی فێربوون و نمایشکردن دروست کراوە. بۆ بەکارهێنانی production، پێویستە تایبەتمەندییە زیاترەکان وەک JWT authentication، database integration، و security measures زیاد بکرێت.

