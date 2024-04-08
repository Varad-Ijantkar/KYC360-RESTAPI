# KYC360-RESTAPI Documentation

## Overview

The KYC360-RESTAPI is designed to provide a comprehensive compliance solution through a RESTful interface. It allows for efficient management of entity data, including creation, modification, deletion, and retrieval with advanced search and filtering capabilities.

## Features

- **CRUD Operations**: Manage entities with endpoints for creating, reading, updating, and deleting records.
- **Search**: Robust search functionality to locate entities by names and addresses.
- **Filtering**: Filter entities based on gender, date ranges, and countries.
- **Pagination & Sorting**: Navigate through large datasets with ease.
- **Retry & Backoff**: Handle transient database errors with a retry mechanism and exponential backoff strategy.

## Getting Started

1. **Installation**
   - Clone the repository: `git clone https://github.com/yourusername/KYC360-RESTAPI.git`
   - Navigate to the project directory: `cd KYC360-RESTAPI`
   - Install dependencies: `dotnet restore`

2. **Configuration**
   - Set up your database connection string in `appsettings.json`.
   - Configure any additional services as required.

3. **Running the API**
   - Start the API: `dotnet run`
   - The API will be available at `http://localhost:5000`.

## API Endpoints

### Entities

- **List Entities**: `GET /api/entities`
- **Get Single Entity**: `GET /api/entities/{id}`
- **Create Entity**: `POST /api/entities`
- **Update Entity**: `PUT /api/entities/{id}`
- **Delete Entity**: `DELETE /api/entities/{id}`

### Search & Filter

- **Search Entities**: `GET /api/entities?search=query`
- **Filter Entities**: `GET /api/entities?gender=male&startDate=yyyy-mm-dd&endDate=yyyy-mm-dd&countries=country1,country2`

## Retry & Backoff Strategy

- **Mechanism**: The API will retry failed database operations up to 3 times.
- **Strategy**: Delays between retries follow an exponential backoff, starting at 2 seconds and doubling each time.

## Testing

- Run tests using the command: `dotnet test`
- Ensure all tests pass before deploying to production.

## Contributing

Contributions are welcome. Please fork the repository, make your changes, and submit a pull request.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

For more detailed information, please refer to the individual sections of this documentation.
