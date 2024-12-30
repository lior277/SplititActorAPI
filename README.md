# SplititActorAPI

C# scraper Api to retrieve information about the top actors from IMDb Within RESTful API

## Project Title
Splitit Actor API

## Description

## Features

## Getting Started

### Prerequisites
- .NET 9 SDK

### Installation
1. Clone the repository:

### Steps to Add Planned Features
Implement a caching mechanism using Redis, where the scraper saves the
scraped data directly into the database. At scheduled intervals, the
data is synchronized with Redis. Subsequent
GET requests will fetch data directly from Redis to improve response
time and reduce database load.

 implemented global authorization, allowing users to authenticate once
 and maintain their session across multiple requests.

 ActorService: this could be
 an ideal location for orchestrating more complex business logic related to actors.
 For instance, validating the actors' data before saving or performing more advanced
 filtering operations. Currently, the logic for scraping actors resides within the actor providers.

 update the layers
 1.Presentation Layer (API Layer)
 2. Service Layer (Business Logic Layer)
 3. Repository Layer (Data Access Layer)
 4.Data Layer
 5.External Service Layer (Web Scraping Providers)
 6.Utility Layer (Support Services, PlaywrightService, ILogger)
 

    
