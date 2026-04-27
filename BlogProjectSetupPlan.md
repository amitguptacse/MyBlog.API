# Blog Project - Complete Setup Plan

**Tech Stack:** Next.js (TypeScript) + ASP.NET Core Web API + SQL Server  
**Hosting:** Vercel (Frontend) + Azure (Backend) | Free Tier  

---

## Table of Contents
- Phase 1: Tech Stack Overview
- Phase 2: Development Environment Setup
- Phase 3: Backend - ASP.NET Core Web API
- Phase 4: Frontend - Next.js + TypeScript
- Phase 5: Features List
- Phase 6: Database Design
- Phase 7: Deployment Architecture
- Phase 8: Project Timeline
- Phase 9: Azure Free Tier Details

---

## Phase 1: Tech Stack Overview

| Layer | Technology | Purpose | Hosting |
|------|-----------|--------|--------|
| Frontend | Next.js + TypeScript + Tailwind CSS | UI, SEO, Fast Pages | Vercel (Free) |
| Backend | ASP.NET Core Web API (C#) | Business Logic, Auth, APIs | Azure F1 (Free) |
| Database | SQL Server / Azure SQL | Store all data | Azure Free Tier |
| CMS (Optional) | Sanity | Easy blog editing | Free Tier |
| Images | Cloudinary | Image upload & CDN | Free (25 GB) |
| Emails | SendGrid | Newsletter, notifications | Free (100/day) |
| Version Control | GitHub | Code & auto-deploy | Free |
| Domain | Namecheap / GoDaddy | Custom domain | ~Rs. 800/year |

---

## Phase 2: Development Environment Setup

### Software to Install
- .NET 8 SDK  
- Node.js LTS  
- Visual Studio 2022 Community  
- SQL Server Express  
- Azure Data Studio  
- Postman  
- Git  

### Accounts to Create
- GitHub  
- Azure  
- Vercel  
- Cloudinary  
- SendGrid  
- Sanity (Optional)  

---

## Phase 3: Backend - ASP.NET Core Web API

### Project Commands
```bash
mkdir MyBlog && cd MyBlog
dotnet new sln -n MyBlog
dotnet new webapi -n MyBlog.API -controllers
dotnet sln add MyBlog.API
cd MyBlog.API
```

### Packages
- EntityFrameworkCore
- SqlServer
- Tools
- JWT Authentication
- BCrypt
- Slugify

---

### Folder Structure
```
MyBlog.API/
├── Controllers/
├── Models/
├── DTOs/
├── Services/
├── Middleware/
├── Data/
├── Helpers/
└── Program.cs
```

---

### API Endpoints

#### Authentication
- POST `/api/auth/register`
- POST `/api/auth/login`
- POST `/api/auth/refresh-token`
- POST `/api/auth/forgot-password`
- GET `/api/auth/me`

#### Blog Posts
- GET `/api/posts`
- GET `/api/posts/{slug}`
- POST `/api/posts`
- PUT `/api/posts/{id}`
- DELETE `/api/posts/{id}`

---

## Phase 4: Frontend - Next.js

### Setup
```bash
npx create-next-app@latest blog-frontend
cd blog-frontend
npm install axios react-query
```

---

### Pages
- `/` → Home  
- `/blog` → All posts  
- `/blog/[slug]` → Single post  
- `/admin` → Admin panel  

---

## Phase 5: Features

### Core
- Blog CRUD
- Markdown editor
- Image upload
- JWT Authentication
- Pagination

### SEO
- Meta tags
- Sitemap
- RSS feed
- Clean URLs

### UX
- Dark mode
- Reading progress
- Search
- Responsive design

### Admin
- Dashboard
- Post management
- Comment moderation

### Security
- JWT Auth
- Rate limiting
- XSS protection

---

## Phase 6: Database Design

### Tables
- Users  
- BlogPosts  
- Categories  
- Tags  
- Comments  
- Newsletter  
- Media  

---

## Phase 7: Deployment

### Frontend (Vercel)
- Connect GitHub
- Auto deploy

### Backend (Azure)
- App Service (F1 Free)
- Azure SQL

---

## Phase 8: Timeline

| Week | Tasks |
|------|------|
| 1-2 | Setup |
| 3-4 | API |
| 5-6 | Frontend |
| 7-8 | SEO |
| 9-10 | Deploy |

---

## Phase 9: Azure Free Tier

### Free Services
- App Service F1  
- Azure SQL  
- Functions  
- Blob Storage  

---

## Final Cost
**~ Rs. 67/month (only domain)**

---

## 🚀 Final Note
All the best for your Blog Project! Happy Coding!
