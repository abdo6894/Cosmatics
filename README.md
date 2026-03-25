# Cosmatics


## Project Overview
**Cosmatics API** is a robust backend service built with **.NET Core (Available on .NET 6+)** designed to power a cosmetics e-commerce platform. It provides comprehensive endpoints for user authentication, product management, shopping cart operations, and order processing.

The project is structured using the **Controller-Service-Repository** pattern to ensure separation of concerns and maintainability.

---

## 🚀 Getting Started

### Prerequisites
- **.NET SDK** (6.0 or later)
- **SQL Server** (LocalDB or full instance)
- **Visual Studio** or **VS Code**

### Configuration
Before running the application, update the `appsettings.json` file with your local configurations:

1.  **Database Connection**:
    Update `ConnectionStrings:DefaultConnection` with your SQL Server connection string.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER;Database=Cosmatics;User Id=user;Password=pass;TrustServerCertificate=True;"
    }
    ```

2.  **Email Settings (SMTP)**:
    The project uses Gmail SMTP for sending OTPs. Update specific credentials in `EmailSettings`:
    ```json
    "EmailSettings": {
      "SmtpServer": "smtp.gmail.com",
      "Port": 587,
      "SenderEmail": "your-email@gmail.com",
      "Password": "your-app-password" 
    }
    ```
    *> Note: For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833?hl=en) if 2FA is enabled.*

3.  **JWT Settings**:
    Ensure the `Jwt:Key` is secure and matches your environment needs.

### Running the Project
1.  **Restore Dependencies**:
    ```bash
    dotnet restore
    ```
2.  **Apply Migrations**:
    Ensure your database is up to date.
    ```bash
    dotnet ef database update
    ```
3.  **Run the Application**:
    ```bash
    dotnet run
    ```
    The API will start (default port is usually `http://localhost:5000` or `https://localhost:7001`).
    Access **Swagger UI** at `/swagger/index.html` for interactive API documentation.

---

## 🏗 Project Architecture

### Tech Stack
-   **Framework**: ASP.NET Core Web API
-   **Database**: SQL Server
-   **ORM**: Entity Framework Core
-   **Authentication**: JWT Bearer Tokens
-   **Logging**: Serilog (Console & File logs in `logs/` folder)
-   **Documentation**: Swagger / OpenAPI

### Folder Structure
-   **`Controllers/`**: Hanldes HTTP requests and defines API endpoints.
-   **`Services/`**: Contains business logic (`AuthService`, `ProductService`, `EmailService`, etc.).
-   **`Repositories/`**: Generic Repository pattern implementation (`IRepository<T>`).
-   **`Models/`**: Database entities (`User`, `Product`, `Order`, etc.).
-   **`DTOs/`**: Data Transfer Objects for request/response payloads.
-   **`Data/`**: `AppDbContext` configuration.
-   **`Migrations/`**: EF Core database schema history.

---

## 🔑 Key Features & Flows

### 1. Authentication & Security
-   **Registration**: Users register with Phone, Email, Username, and Country Code.
-   **OTP Verification**: 
    -   A 4-digit One-Time Password (OTP) is generated and sent via Email.
    -   Users must verify their account before logging in.
-   **Login**:
    -   Supports login via **Username** OR **Phone Number**.
    -   Returns a JWT Token on success.
-   **Single Session Enforcement**:
    -   The system tracks an `ActiveToken`. Logging in from a new device invalidates previous sessions.
    -   `TokenValidationMiddleware` ensures only the current active token is valid.
-   **Password Management**:
    -   Secure password hashing (HMACSHA512).
    -   Forgot Password flow sends OTP to email for resetting.

### 2. User Roles
-   **Customer**: Standard user who can browse products, manage cart, and place orders.
-   **Admin**: (Implied) Has higher privileges for managing resources (e.g., Delete All Users).

### 3. E-Commerce Features
-   **Products & Categories**:
    -   Full CRUD operations.
    -   Publicly accessible endpoints for listing products (bypasses Auth).
-   **Shopping Cart**:
    -   Manage cart items persistently.
-   **Orders**:
    -   Place orders from cart contents.
-   **Sliders**:
    -   Manage homepage banner images/offers.

---

## 📚 API Overview

### **Auth** (`/api/auth`)
-   `POST /register`: Create a new account.
-   `POST /login`: Authenticate and receive JWT.
-   `POST /verify-otp`: Verify account registration.
-   `POST /forgot-password`: Initiate password reset.
-   `POST /reset-password`: Complete password reset.
-   `POST /resend-otp`: Send a new verification code.
-   `POST /logout`: Invalidate current session.

### **Profile** (`/api/profile`)
-   `GET /me`: Get current user details.
-   `PUT /update`: Update profile info (Photo, Email, Username).
-   `POST /change-password`: Change authenticated user's password.

### **Products** (`/api/products`)
-   `GET /`: List all products.
-   `GET /{id}`: Get product details.
-   `POST /`: Add new product (Admin).
-   `PUT /`: Update product.
-   `DELETE /`: Remove product.

### **Cart** (`/api/cart`)
-   `GET /`: View cart.
-   `POST /add`: Add item to cart.
-   `DELETE /remove`: Remove item.
-   `POST /checkout`: (If applicable)

### **Admin / Utilities**
-   `DELETE /api/auth/delete-user/{id}`: Admin delete user.
-   `GET /api/countries`: List supported country codes.

---

## 🛠 Database Schema (Key Entities)

### **User**
| Column | Type | Description |
| :--- | :--- | :--- |
| `Id` | int | Primary Key |
| `Username` | string | Unique identifier |
| `Email` | string | Used for OTP/Notifications |
| `PhoneNumber` | string | Login identifier |
| `CountryCode` | string | e.g., "+20" |
| `PasswordHash` | string | Salted & Hashed |
| `IsVerified` | bool | OTP verification status |
| `ActiveToken` | string | For session management |

### **Product**
| Column | Type | Description |
| :--- | :--- | :--- |
| `Id` | int | Primary Key |
| `Name` | string | Product Name |
| `Price` | decimal | |
| `Stock` | int | Inventory count |
| `ImageUrl` | string | URL to product image |
| `CategoryId` | int | Foreign Key |

---

## 📝 Developer Notes for Frontend
1.  **Base URL**: Use the local URL (e.g., `https://localhost:7001`).
2.  **Headers**:
    -   For protected endpoints, include `Authorization: Bearer <TOKEN>` header.
3.  **Images**:
    -   Product and User images are currently stored as URLs (strings). Ensure the UI handles valid URL strings.
4.  **Error Handling**:
    -   API usage standardized error messages.
    -   `401 Unauthorized`: Token invalid or expired (or logged out elsewhere).
    -   `400 Bad Request`: Validation errors.

---
*Generated for the Cosmatics Team*
