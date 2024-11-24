# UserService - Airline Reservation System
**UserService** handles user registration, authentication, and role-based access control (RBAC) in the Airline Reservation System. It provides endpoints for login, user registration, and secure JWT token generation. The service listens on port 5001 but can also be accessed through the **API Gateway** running on port 5000.
## Key Features

- **User Authentication**: Login and JWT token generation.
- **User Registration**: Create new user accounts.
- **Role-based Access Control (RBAC)**: Enforce access control based on user roles.
- **API Gateway Access**: The service is also accessible via the API Gateway on port 5000.

### Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/srvignesh5/UserService.git
   cd UserService
   ```
2. **Restore the NuGet packages**
      ```bash
    dotnet restore
   ```
4. **Run the UserService**
   ```bash
    dotnet run
   ```
   The service will start and listen on https://localhost:5001. However, it is also accessible via the API Gateway on port 5000.

Access the API documentation (Swagger UI) for UserService directly
```bash
https://localhost:5001/swagger
```
Access the API through the API Gateway on port 5000:
```bash
https://localhost:5000
```
